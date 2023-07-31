namespace AppFrame.Interfaces;
public interface ITable<T>
{
    IIndexedLoggedObjectStore ObjectStore { get; }              // this is the object store that is used to store the table 
    Task Upsert(string rowKey, T value, int transactionId);  
    Task Delete(string rowKey, int transactionId);           
    Task<T> Read(string rowKey);          
    Task<IEnumerable<T>> Query(string text);                   
    Task<string> BeginTransaction(); 
    Task<bool> CommitTransaction(string TransactionId); 
    Task<bool> CancelTransaction(string TransactionId); 
}

// public interface ITable
// {
//     IIndexedLoggedObjectStore ObjectStore { get; }              // this is the object store that is used to store the table 
//     Task Upsert(string rowKey, string value, int transactionId);  
//     Task Delete(string rowKey, int transactionId);           
//     Task<string> Read(string rowKey);          
//     Task<IEnumerable<string>> Query(string text);                   
// }