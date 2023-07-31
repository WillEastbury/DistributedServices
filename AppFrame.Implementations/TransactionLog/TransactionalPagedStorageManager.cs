using AppFrame.Interfaces;

namespace AppFrame.Implementations.PageStorage
{
    public class TransactionalPagedStorageManager : ITransactionalPagedStorageManager
    {
        public ITransactionLog _TransactionLog {get; set; }
        public IPagedStorageReplicaSet _ReplicaSet { get; set; }
        private int CommitIntervalInMs { get; set; }
        public TransactionalPagedStorageManager(ITransactionLog transactionLog, IPagedStorageReplicaSet replicaSet, int commitIntervalInMs = 100)
        {
            _TransactionLog = transactionLog;
            _ReplicaSet = replicaSet;
            CommitIntervalInMs = commitIntervalInMs;
        }
        public async Task<int> BeginTransaction()
        {
            return await _TransactionLog.BeginTransaction();
        }
        public async Task<byte[]> GetPageData(string fileId, int pageId)
        {
            // Fetch the page from the main storage replicaset
            byte[] diskpage = await _ReplicaSet.GetPageAsync(fileId, pageId);

            // Now the last committed or completed change from the transaction log that contains that page 
            TransactionLogEntry transactionLogEntry = 
            (await _TransactionLog.LoadTransactionsAffectingPageFromStore(fileId, pageId))
            .Where(e => e.PageUpdateStates.ContainsKey($"{fileId}-{pageId}") && e.CommitState == (CommitState.Committed | CommitState.Completed))
            .OrderByDescending(e => e.TransactionId)
            .First();

            if (transactionLogEntry != null)
            {
                transactionLogEntry.PageUpdateStates.TryGetValue($"{fileId}-{pageId}", out TransactionalPageUpdate transactionalPageUpdate);
                diskpage = transactionalPageUpdate.NewData;
            }
            return diskpage;
        }
        public async Task SetPageData(int transactionId, string fileId, int pageId,  byte[] data, byte[] oldData = null)
        {
           await _TransactionLog.AddLoggedUpdateToTransaction(transactionId, fileId, pageId, data, oldData);
        }
        public async Task SetTransactionState(int transactionId, CommitState commitState)
        {
            // Set the transaction to the appropriate state
            await _TransactionLog.SetTransactionState(transactionId, commitState);

        }
        public async Task BackgroundCommitFlusher(CancellationToken cancellationToken)
        {
            while(cancellationToken.IsCancellationRequested == false)
            {
                // Get the list of transactions that are ready to be committed
                List<TransactionLogEntry> transactionsToCommit = _TransactionLog.ListTransactions().Where(e => e.CommitState == CommitState.Committed).ToList();

                // Atomically walk through and loop through each committed transaction in order
                foreach (TransactionLogEntry transaction in transactionsToCommit)
                {
                    try
                    {
                        // Mark the log entry as being completed so that we can recover it if we crash
                        await _TransactionLog.SetTransactionState(transaction.TransactionId, CommitState.Completing);
                        await _TransactionLog.SaveTransactionToStore(transaction);
                        // for each one perform the update 
                        foreach (TransactionalPageUpdate pageUpdate in transaction.PageUpdateStates.Values)
                        {
                            await _ReplicaSet.SetPageAsync(pageUpdate.FileId, pageUpdate.PageNumber, pageUpdate.NewData);
                        }
                        await _TransactionLog.SetTransactionState(transaction.TransactionId, CommitState.Completed);
                    }
                    catch(Exception tex)
                    {
                        Console.WriteLine($"Transaction {transaction.TransactionId} failed to commit, rolling back. {tex.Message}");
                        // If we fail, we need to mark the transactions as broken so that they can be rolled back and rollback the update                       
                        // IF THIS FAILS HERE WE WANT THE ENGINE TO CRASH HERE WITH AN UNHANDLED EXCEPTION
                        // for each one perform the un-update :)  
                        foreach (TransactionalPageUpdate pageUpdate in transaction.PageUpdateStates.Values)
                        {
                            await _ReplicaSet.SetPageAsync(pageUpdate.FileId, pageUpdate.PageNumber, pageUpdate.OldData);
                        }
                        await _TransactionLog.SetTransactionState(transaction.TransactionId, CommitState.Broken);
                        Console.WriteLine($"Transaction {transaction.TransactionId} rollback complete");
                    }

                    // Save the state of the transaction now we are all done. 
                    await _TransactionLog.SaveTransactionToStore(transaction);
                }

                // Sleep till it's time to scan again! 
                await Task.Delay(CommitIntervalInMs, cancellationToken);
            }
            Console.WriteLine("Exiting Commit Flusher, Shutdown Requested.");
        }
        public async Task DeletePage(int transactionId, string fileId, int pageId)
        {
            await _TransactionLog.AddLoggedUpdateToTransaction(transactionId, fileId, pageId, await GetPageData(fileId, pageId), null);
        }
    }
}