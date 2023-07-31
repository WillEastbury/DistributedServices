using System.Threading.Tasks;
namespace AppFrame.Interfaces;

public interface ILoggedKeyValueStore 
{
    // This level of abstraction is needed to allow for different implementations of the transaction log and the paged storage manager
    // we rely on the transaction log to provide the transaction id and the commit state for each transaction
    // we rely on the paged storage manager to handle the replica storage and the transactional updates to the pages

    // This class needs to handle the following:
    // Indexing of the different key values for the KV Store (this is done by the search trees)
    // Caching of the key values (this is done by the key value cache)
    // Transactional updates to the key values (this is done by the transaction log and the paged storage manager)
    // Transactional updates to the search trees (this is done by the transaction log and the paged storage manager)
       
    ITransactionalPagedStorageManager transactionalPagedStorageManager { get; }              
    Task<bool> Exists(string key);
    Task<byte[]> Read(string key);
    Task Upsert(int transactionId, string key, byte[] value);
    Task Delete(int transactionId, string key);
    Task Patch(int transactionId, string key, PatchInstruction[] patchInstructions);
    Task Append(int transactionId, string key, byte[] value);
    Task<string> BeginTransaction();
    Task<bool> CommitTransaction(string TransactionId); 
    Task<bool> CancelTransaction(string TransactionId); 
}

