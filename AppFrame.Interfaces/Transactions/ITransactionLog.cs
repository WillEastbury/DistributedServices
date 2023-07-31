namespace AppFrame.Interfaces;
public interface ITransactionLog
{
    IPagedStorageReplicaSet DataPagesReplicaSet { get; }
    Task<int> BeginTransaction();
    Task AddLoggedUpdateToTransaction(int transactionId, string fileId, int pageId, byte[] newData, byte[] oldData = null);
    Task SetTransactionState(int transactionId, CommitState commitState);
    Task UpdateLogHeader();
    Task<TransactionLogEntry> LoadTransactionFromStore(int transactionId);
    Task<IEnumerable<TransactionLogEntry>> LoadTransactionsAffectingPageFromStore(string fileId, int pageId);
    Task SaveTransactionToStore(TransactionLogEntry transactionLogEntry);
    IReadOnlyList<TransactionLogEntry> ListTransactions(CommitState commitState = CommitState.Prepared | CommitState.NotCommitted | CommitState.Committed, int startFromTransactionId = -1);
    Task FlushLog(int startFromTransactionId = -1);
}