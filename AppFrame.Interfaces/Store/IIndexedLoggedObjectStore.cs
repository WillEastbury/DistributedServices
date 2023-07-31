using System.Reflection;
using AppFrame.Expressions;

namespace AppFrame.Interfaces;
public interface IIndexedLoggedObjectStore
{
    ILoggedKeyValueStore lkvs { get; }
    Dictionary<string, ISearchTree> searchTrees { get; }
    Task Upsert(int transactionId, string key, string value);
    Task Delete(int transactionId, string key);
    Task<string> Read(string key);
    Task<bool> Exists(string key);
    Task<IEnumerable<byte[]>> Query(IEnumerable<WhereExpressionComparison> expressions);
    Task<string> BeginTransaction();
    Task<bool> CommitTransaction(string TransactionId); 
    Task<bool> CancelTransaction(string TransactionId); 
}