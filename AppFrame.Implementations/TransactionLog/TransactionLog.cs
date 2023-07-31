using AppFrame.Interfaces;
using AppFrame.Paging;
using System.Diagnostics;
namespace AppFrame.Implementations.TransactionLog;
public class TransactionLog : ITransactionLog
{
    // The basic storage for the transaction log is a list of transactions stored in memory, but hardened to a set of replicas. 
    // This only makes sense to be used for either a sequential event log of transactions for a write-ahead-log for a data store.
    // The transaction log does need to store some metadata about itself for this to make sense. 
    // We need to know which file ID to use for the transaction log, and we need to know which file ID to use for the transaction log index.
    // We also need to know which page ID to use for the start of the transaction log index and the data pages.
    // We also need to know where the committed tail of the log that has been checkpointed is stored (It kind of makes sense to store this on page zero of file zero to me). 
    private IDictionary<string, TransactionLogEntry> TransactionLogCache { get; set; } = new Dictionary<string, TransactionLogEntry>();
    public IPagedStorageReplicaSet DataPagesReplicaSet {get;set;}
    private RootTransactionLogPage RootPage {get;set;} = new RootTransactionLogPage();
    public TransactionLog(IPagedStorageReplicaSet dataPagesReplicaSet)
    {
        this.DataPagesReplicaSet = dataPagesReplicaSet;

        // At startup, we need to initialize the transaction log by bootstrapping page zero on file zero with the transaction log metadata.
        using Stream pageStream = InitializeLogHeader(dataPagesReplicaSet);
    }
    private Stream InitializeLogHeader(IPagedStorageReplicaSet dataPagesReplicaSet)
    {
        // if [page zero of file zero] is empty, then initialize it with the transaction log metadata, as this is a new log
        // if [page zero of file zero] is not empty, then read the transaction log metadata from it.
        // if the transaction log metadata is not valid, then throw an exception.
        // if the transaction log metadata is valid, then use it to initialize the transaction log.

        //byte[] pageZero = ReplicaSet.GetPageAsync(0, 0).Result; // Should be a 13 byte page. 
        Stream pageStream = dataPagesReplicaSet.GetPageStreamAsync("TransactionLog", 0).Result;

        // if this is not zero bytes or null
        if (pageStream != null && pageStream.Length == 13)
        {
            using (BinaryReader reader = new(pageStream))
            {
                RootPage = new RootTransactionLogPage
                {
                    // Read the values from the file
                    LogLengthInPages = reader.ReadInt32(),
                    LogStartPageNumber = reader.ReadInt32(),
                    LogEndPageNumber = reader.ReadInt32(),
                    NextTransactionId = reader.ReadInt32(),
                    Dirty = reader.ReadBoolean()
                };
            }
            UpdateLogHeader().Wait();
        }
        else
        {
            throw new Exception("Invalid transaction log header, not null and not 13 bytes long.");
        }
        return pageStream;
    }
    public async Task UpdateLogHeader()
    {
        // There are a LOT of these to write, so we'd better be as efficient as possible.
        // using MemoryStream memoryStream = new();
        // using BinaryWriter writer = new(memoryStream); 
        // {
        //     // Write away the transaction log metadata to page zero of file zero.
        //     writer.Write(RootPage.LogLengthInPages);
        //     writer.Write(RootPage.LogStartPageNumber);
        //     writer.Write(RootPage.LogEndPageNumber);
        //     writer.Write(RootPage.NextTransactionId);
        //     writer.Write(RootPage.Dirty);
        // }
        // await ReplicaSet.SetPageStreamAsync(0,0, memoryStream);

        // Raw buffer operations for speed
        const int logHeaderSize = sizeof(int) * 4 + sizeof(bool);
        byte[] buffer = new byte[logHeaderSize];

        // Copy the log header data directly to the buffer
        int offset = 0;
        Buffer.BlockCopy(BitConverter.GetBytes(RootPage.LogLengthInPages), 0, buffer, offset, sizeof(int));
        offset += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(RootPage.LogStartPageNumber), 0, buffer, offset, sizeof(int));
        offset += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(RootPage.LogEndPageNumber), 0, buffer, offset, sizeof(int));
        offset += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(RootPage.NextTransactionId), 0, buffer, offset, sizeof(int));
        offset += sizeof(int);
        Buffer.BlockCopy(BitConverter.GetBytes(RootPage.Dirty), 0, buffer, offset, sizeof(bool));
        await DataPagesReplicaSet.SetPageAsync("TransactionLog", 0, buffer);

    }
    public async Task AddLoggedUpdateToTransaction(int transactionId, string fileId, int pageId, byte[] newdata, byte[] olddata = null)
    {

        // Ensure there is no uncommitted AND NON completed good or broken transactions in flight for the same file and pageId.
        // This is a very important check, as it prevents the transaction log from being corrupted by multiple transactions updating the same page at the same time.

        if ((await LoadTransactionsAffectingPageFromStore(fileId, pageId))
            .Where(e => 
                e.PageUpdateStates.ContainsKey($"{fileId}-{pageId}") 
                && 
                e.CommitState != (CommitState.Committed | CommitState.Completed | CommitState.Broken)
            )
            .Any()
        )
        {
            throw new Exception($"There is already an uncommitted transaction in flight for this file-page ({fileId}-{pageId}) wait (up to 10s) for the transaction to commit or timeout and retry.");
        }

        // Check the size of the requested update is not too large for the transaction log.
        // 31K is the maximum size (31k for old record and 31k for new record + 2k for header = 64k)
        if(newdata.Length > 31744)
        {
            throw new Exception("The size of the new data is too large to be stored in the transaction log in a single data page.");
        }
        if(olddata != null && olddata.Length > 31744)
        {
            throw new Exception("The size of the old data is too large to be stored in the transaction log in a single data page.");
        }
        
        // Firstly load the transaction data from the transaction log.
        TransactionLogEntry transactionLogEntry = await LoadTransactionFromStore(transactionId);

        // check the log entry is in a valid state to be updated (not failed or prepared or committed)
        if(transactionLogEntry.CommitState == CommitState.NotCommitted)
        {
            throw new Exception($"Transaction log entry is not in a valid state to be updated ({transactionLogEntry.CommitState}).");
        }

        // Split both new and old data into 31K data pages and diff them 
        // (we need to do this to ensure that we can store the data in the transaction log)
        // Then add only the changed pages to the transaction log entry.

        // // // Diff the new and old pages 
        // // // IEnumerable<PageChange> pageDiffs = await Utilities.Utilities.GetPageSetChanges(await Utilities.Utilities.SplitIntoDataPages(olddata), await Utilities.Utilities.SplitIntoDataPages(newdata));

        // // // Add the page changes to the transaction log entry
        // // // foreach(PageChange pageChange in pageDiffs)
        // // // {

        // Was this page already changed by this transaction, if so overwrite the page update? 
        if(transactionLogEntry.PageUpdateStates.TryGetValue($"{fileId}-{pageId}", out TransactionalPageUpdate tr))
        {
            if(olddata != null && tr.NewData != olddata)
            {
                throw new Exception("Consistency problem, the old data does not match the present data in the transaction log for this page.");
            }

            tr.NewData = newdata;
            tr.OldData = olddata;
            tr.PageOldVersion = tr.PageNewVersion;
            tr.PageNewVersion = tr.PageOldVersion + 1;
            tr.CommitState = CommitState.NotCommitted;
        }
        else
        {
            // Log the new page update
            transactionLogEntry.PageUpdateStates[$"{fileId}-{pageId}"] = new TransactionalPageUpdate(
                    fileId,
                    pageId,
                    newdata,
                    olddata
                )
            {
                PageOldVersion = 0,
                PageNewVersion = 1,
                CommitState = CommitState.NotCommitted
            };
        }
        // }

        await SaveTransactionToStore(transactionLogEntry);
    }
    public async Task<int> BeginTransaction()
    {
        // Get the next transaction ID from the root page
        int transactionId = RootPage.GetNextTransactionId();

        // Create the transaction record 
        TransactionLogEntry transactionLogEntry = new()
        {
            TransactionId = transactionId,
            PageUpdateStates = new Dictionary<string, TransactionalPageUpdate>(), 
            CommitState = CommitState.NotCommitted
        };

        // Add the initial transaction record to the transaction log 
        await SaveTransactionToStore(transactionLogEntry);

        // Return the transaction ID
        return transactionId; 
    }
    public Task<IEnumerable<TransactionLogEntry>> LoadTransactionsAffectingPageFromStore(string fileId, int pageId)
    {   
        // Return all transactions that have a page update for the key that are completed to disk or committed to the log
        return Task.FromResult(ListTransactions(CommitState.Committed | CommitState.Completed).Where(e => e.PageUpdateStates.ContainsKey($"{fileId}-{pageId}")).AsEnumerable());
    }
    public async Task<TransactionLogEntry> LoadTransactionFromStore(int transactionId)
    {

        // Try the cache first, if not in the cache then load from the replicaset
        if (!TransactionLogCache.TryGetValue(transactionId.ToString(), out TransactionLogEntry transactionLogEntry))
        {
            // Page Zero is always the Header to the transaction log entry from the replicaset
            byte[] transactionPageList = await DataPagesReplicaSet.GetPageAsync(transactionId.ToString(), 0);

            // Read the transaction log header page from the replicaset
            int transactionIdLoaded = BitConverter.ToInt32(transactionPageList, 0);
            int numberOfDataPages = BitConverter.ToInt32(transactionPageList, 4);
            transactionLogEntry.CommitState = (CommitState)BitConverter.ToInt32(transactionPageList, 8);
            transactionLogEntry.BeganAtUTC = DateTime.FromBinary(BitConverter.ToInt64(transactionPageList, 12));

            // Check the transaction ID matches the transaction ID requested
            if (transactionIdLoaded != transactionId)
            {
                Debug.Assert(transactionIdLoaded == transactionId);
            }

            // Now load the pages from the replicaset 
            transactionLogEntry.PageUpdateStates = new Dictionary<string, TransactionalPageUpdate>();

            for (int i = 1; i < numberOfDataPages + 1; i++)
            {
                // Read the page from the replicaset
                byte[] pageData = await DataPagesReplicaSet.GetPageAsync(transactionId.ToString(), i);

                TransactionalPageUpdate tpthis = new()
                {
                    FileId = BitConverter.ToInt32(pageData, 0).ToString(),
                    PageNumber = BitConverter.ToInt32(pageData, 4),
                    CommitState = (CommitState)BitConverter.ToInt32(pageData, 8),
                    PageNewVersion = BitConverter.ToInt32(pageData, 12),
                    PageOldVersion = BitConverter.ToInt32(pageData, 16)
                };

                int oldDataLength = BitConverter.ToInt32(pageData, 20);
                int newDataLength = BitConverter.ToInt32(pageData, 24);
                
                if (oldDataLength > 0)
                {
                    // Read the old data
                    byte[] oldData = new byte[oldDataLength];
                    Buffer.BlockCopy(pageData, 25, oldData, 0, oldDataLength);
                }

                if (newDataLength > 0)
                {
                    // Read the new data
                    byte[] newData = new byte[newDataLength];
                    Buffer.BlockCopy(pageData, 25 + oldDataLength, newData, 0, newDataLength);
                }

                // Add the page update to the list
                transactionLogEntry.PageUpdateStates.Add($"{tpthis.FileId}-{tpthis.PageNumber}", tpthis);
            }

            // Add the transaction log entry to the cache
            TransactionLogCache[$"{0}-{0}"] = transactionLogEntry;
        }

        return transactionLogEntry; 
    }
    public async Task SaveTransactionToStore(TransactionLogEntry transactionLogEntry)
    {
        // Create a new buffer to hold the transaction header page
        byte[] headerPage = new byte[12];
        Buffer.BlockCopy(BitConverter.GetBytes(transactionLogEntry.TransactionId), 0, headerPage, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transactionLogEntry.PageUpdateStates.Count), 0, headerPage, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((int)transactionLogEntry.CommitState), 0, headerPage, 8, 4);
        byte[] utcDateTimeBytes = BitConverter.GetBytes(transactionLogEntry.BeganAtUTC.ToBinary());
        Buffer.BlockCopy(utcDateTimeBytes, 0, headerPage, 12, utcDateTimeBytes.Length);

        // Save the transaction header page to the replicaset
        await DataPagesReplicaSet.SetPageAsync(transactionLogEntry.TransactionId.ToString(), 0, headerPage);

        // Save the individual page updates
        foreach (var pageUpdateState in transactionLogEntry.PageUpdateStates.Values)
        {
            // Calculate the size of the page data
            int pageDataSize = 25; // Minimum size for file ID, page number, commit state, page versions, and data length fields
            if (pageUpdateState.OldData != null)
                pageDataSize += pageUpdateState.OldData.Length;
            if (pageUpdateState.NewData != null)
                pageDataSize += pageUpdateState.NewData.Length;

            // Create a buffer to hold the page data
            byte[] pageData = new byte[pageDataSize];

            // Copy the page update state to the buffer
            Buffer.BlockCopy(BitConverter.GetBytes(int.Parse(pageUpdateState.FileId)), 0, pageData, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pageUpdateState.PageNumber), 0, pageData, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int)pageUpdateState.CommitState), 0, pageData, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pageUpdateState.PageOldVersion), 0, pageData, 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pageUpdateState.PageNewVersion), 0, pageData, 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pageUpdateState.OldData?.Length ?? 0), 0, pageData, 20, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pageUpdateState.NewData?.Length ?? 0), 0, pageData, 24, 4);

            // Copy the old data to the buffer if present
            if (pageUpdateState.OldData != null)
                Buffer.BlockCopy(pageUpdateState.OldData, 0, pageData, 25, pageUpdateState.OldData.Length);

            // Copy the new data to the buffer if present
            if (pageUpdateState.NewData != null)
                Buffer.BlockCopy(pageUpdateState.NewData, 0, pageData, 25 + (pageUpdateState.OldData?.Length ?? 0), pageUpdateState.NewData.Length);

            // Save the page data to the replicaset
            await DataPagesReplicaSet.SetPageAsync(transactionLogEntry.TransactionId.ToString(), pageUpdateState.PageNumber, pageData);
        }
    }
    public IReadOnlyList<TransactionLogEntry> ListTransactions(CommitState commitState = CommitState.Prepared | CommitState.NotCommitted | CommitState.Committed, int startFromTransactionId = -1)
    {
        return TransactionLogCache.Values.Where(t => (t.CommitState & commitState) == t.CommitState && t.TransactionId >= startFromTransactionId).ToList();
    }
    public async Task SetTransactionState(int transactionId, CommitState commitState)
    {
        TransactionLogEntry transactionLogEntry = await LoadTransactionFromStore(transactionId);
        transactionLogEntry.CommitState = commitState;
        await SaveTransactionToStore(transactionLogEntry);
    }
    public async Task FlushLog(int startFromTransactionId = -1)
    {
       // Traverse the log from start to end, removing any transactions that are flushed to disk (State = Completed)
         foreach(var transactionLogEntry in TransactionLogCache.Values.Where(t => t.TransactionId >= startFromTransactionId && t.CommitState == (CommitState.Broken & CommitState.Completed)).OrderBy(t => t.TransactionId))
         {
            TransactionLogCache.Remove(transactionLogEntry.TransactionId.ToString());
            // Now remove all of the pages for this transaction from the replicaset
            for(int i = 0; i < transactionLogEntry.PageUpdateStates.Count; i++)
            {
                await DataPagesReplicaSet.SetPageAsync(transactionLogEntry.TransactionId.ToString(), i, null);
            }
         }
    }
    public async Task CommitTimeoutMonitor()
    {
        // Scan the log for any transactions that are in the uncommitted state and are older than 10 seconds according to the BeganAtUTC flag and roll them back 
        foreach(var transactionLogEntry in TransactionLogCache.Values.Where(t => t.CommitState == CommitState.NotCommitted && t.BeganAtUTC < DateTime.UtcNow.AddSeconds(-10)))
        {
            // Timeout the transaction
            await SetTransactionState(transactionLogEntry.TransactionId, CommitState.TimedOut);
        }
    }
}