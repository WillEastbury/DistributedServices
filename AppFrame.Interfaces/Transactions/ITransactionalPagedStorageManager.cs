namespace AppFrame.Interfaces
{
    public interface ITransactionalPagedStorageManager
    {
        ITransactionLog _TransactionLog {get;set;}
        IPagedStorageReplicaSet _ReplicaSet {get;set;}

        // Read Methods
        Task<byte[]> GetPageData(string fileId, int pageId);

        // Update Methods
        Task<int> BeginTransaction();
        Task DeletePage(int transactionId, string fileId, int pageId);
        Task SetPageData(int transactionId, string fileId, int pageId,  byte[] data, byte[] oldData = null);
        Task SetTransactionState(int transactionId, CommitState commitState);

        Task BackgroundCommitFlusher(CancellationToken cancellationToken);
    }


}