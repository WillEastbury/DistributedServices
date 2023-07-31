using AppFrame.Interfaces;
namespace AppFrame.Implementations
{
    public class LoggedKeyValueStore : ILoggedKeyValueStore
    {
        // This TransactionalPagedStorageManager to implement the ILoggedKeyValueStore interface
        // this gives us a transactional key value store that can be used to HARNESS the transaction log and the paged storage manager to store data in a reliable way

        // and we need to know how many pages are in each file / object / mapping

        // Caching is handled further down the stack for us by the transactional paged storage manager's transaction log and the replica instances

        public int PageSize {get;set;} = 32768;
        public ITransactionalPagedStorageManager transactionalPagedStorageManager {get;set;}
        public LoggedKeyValueStore(ITransactionalPagedStorageManager transactionalPagedStorageManager)
        {
            this.transactionalPagedStorageManager = transactionalPagedStorageManager;
        }

        public async Task<string> BeginTransaction(){

            return (await transactionalPagedStorageManager.BeginTransaction()).ToString();

        }

        public async Task<bool> CommitTransaction(string TransactionId){

            await transactionalPagedStorageManager.SetTransactionState(int.Parse(TransactionId), CommitState.Committed);
            return true;

        }

        public async Task<bool> CancelTransaction(string TransactionId){

            await transactionalPagedStorageManager.SetTransactionState(int.Parse(TransactionId), CommitState.Broken);
            return true;

        }


        public async Task<bool> Exists(string key)
        {
            return await Read(key) != null;
        }
        public async Task<byte[]> Read(string key)
        {
            // Implement read logic by fetching page zero and loading the metadata then fetching the data pages and assembling them into a single byte array
            FileMetadata metadata = FileMetadata.Deserialize(await transactionalPagedStorageManager.GetPageData(key, 0));

            // Fetch the data pages one by one and assemble them into a single byte array
            byte[] data = new byte[metadata.fileOverallLength];
            int offset = 0;

            for (int i = 1; i < metadata.filePageCount + 1; i++)
            {
                (await transactionalPagedStorageManager.GetPageData(key, i)).CopyTo(data, offset);
                offset += PageSize;
            }
            return data; 
        }

        public async Task Upsert(int transactionId, string key, byte[] value)
        {
            // Implement insert logic by creating a new key file and writing the data to it in pages
            FileMetadata metadata = new()
            {
                fileOverallLength = value.Length,
                filePageCount = (int)Math.Ceiling((double)value.Length / PageSize),
                startOffsetInPages = 1,
                FileId = key
            };
            await transactionalPagedStorageManager.SetPageData(transactionId, key, 0, FileMetadata.Serialize(metadata));

            for (int i = 1; i < metadata.filePageCount + 1; i++)
            {
                byte[] pageData = new byte[PageSize];
                Array.Copy(value, (i - 1) * PageSize, pageData, 0, Math.Min(PageSize, value.Length - ((i - 1) * PageSize)));
                await transactionalPagedStorageManager.SetPageData(transactionId, key, i, pageData);
            }

            // If there are additional pages in the file, delete them
            for (int i = metadata.filePageCount + 1; i < metadata.filePageCount + metadata.startOffsetInPages; i++)
            {
                await transactionalPagedStorageManager.DeletePage(transactionId, key, i);
            }
        }

        public async Task Delete(int transactionId, string key)
        {
            // Implement delete logic by deleting the loading page zero and then removing the pages 
            FileMetadata metadata = FileMetadata.Deserialize(await transactionalPagedStorageManager.GetPageData(key, 0));

            // Delete the pages 
            for (int i = 1; i < metadata.filePageCount + 1; i++)
            {
                await transactionalPagedStorageManager.DeletePage(transactionId, key, i);
            }

        }

        public async Task Patch(int transactionId, string fileId, PatchInstruction[] patchInstructions)
        {
            // Load the necessary pages for patching
            Dictionary<int, byte[]> oldDataPages = new();
            Dictionary<int, byte[]> newDataPages = new();

            foreach (PatchInstruction instruction in patchInstructions)
            {
                int pageNumber = instruction.Offset / PageSize;
                if (!oldDataPages.ContainsKey(pageNumber))
                {
                    byte[] pageData = await transactionalPagedStorageManager.GetPageData(fileId, pageNumber);
                    oldDataPages.Add(pageNumber, pageData);
                    newDataPages.Add(pageNumber, pageData);
                }

                int remainingDataLength = instruction.Data.Length;
                int offsetInPage = instruction.Offset % PageSize;
                int bytesToWrite = Math.Min(PageSize - offsetInPage, remainingDataLength);

                Array.Copy(instruction.Data, 0, newDataPages[pageNumber], offsetInPage, bytesToWrite);
                remainingDataLength -= bytesToWrite;

                while (remainingDataLength > 0)
                {
                    pageNumber++;
                    if (!oldDataPages.ContainsKey(pageNumber))
                    {
                        byte[] pageData = await transactionalPagedStorageManager.GetPageData(fileId, pageNumber);
                        oldDataPages.Add(pageNumber, pageData);
                        newDataPages.Add(pageNumber, pageData);
                    }

                    bytesToWrite = Math.Min(PageSize, remainingDataLength);

                    Array.Copy(instruction.Data, instruction.Data.Length - remainingDataLength, newDataPages[pageNumber], 0, bytesToWrite);
                    remainingDataLength -= bytesToWrite;
                }
            }

            // Write the modified pages back to the storage system
            foreach (KeyValuePair<int, byte[]> kvp in newDataPages)
            {
                int pageNumber = kvp.Key;
                byte[] pageData = kvp.Value;
                await transactionalPagedStorageManager.SetPageData(transactionId, fileId, pageNumber, pageData, oldDataPages[pageNumber]);
            }
        }

        public async Task Append(int transactionId, string key, byte[] newData)
        {
            // Get the metadata for the file and load the last page if not completely filled
            // Fill the supplied data to the next byte after the final page in the file, then continue to write any further data to new pages

            FileMetadata fileMetadata = FileMetadata.Deserialize(await transactionalPagedStorageManager.GetPageData(key, 0));
            int dataPages = fileMetadata.fileOverallLength / PageSize;
            int lastPageLength = fileMetadata.fileOverallLength % PageSize;
            int writtenBytes = 0;
            if (lastPageLength > 0)
            {
                // Overflow last page - load it first and fill it
                byte[] lastPageData = await transactionalPagedStorageManager.GetPageData(key, dataPages);   
                int writeBytes = Math.Min(PageSize - lastPageLength, newData.Length);         
                Array.Copy(newData, 0, lastPageData, lastPageLength, writeBytes);
                writtenBytes += writeBytes;
                await transactionalPagedStorageManager.SetPageData(transactionId, key, dataPages, lastPageData);

            }

            // Just allocate new pages and fill them with any remaining data after the possible overflow above (writtenBytes will be zero if no overflow)
            while (writtenBytes < newData.Length)
            {
                int writeBytes = Math.Min(PageSize, newData.Length - writtenBytes);
                byte[] pageData = new byte[PageSize];
                Array.Copy(newData, writtenBytes, pageData, 0, writeBytes);
                writtenBytes += writeBytes;
                await transactionalPagedStorageManager.SetPageData(transactionId, key, ++dataPages, pageData);
            }
        }

        public Task<bool> SetTransactionState(string TransactionId, CommitState commitState)
        {
            transactionalPagedStorageManager.SetTransactionState(int.Parse(TransactionId), commitState);
            return Task.FromResult(true);
        }
    }
}