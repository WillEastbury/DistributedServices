using System.Text.Json;
using AppFrame.Expressions;
using AppFrame.Interfaces;
namespace AppFrame.Implementations;
public class Table<T> : ITable<T>
{
    public IIndexedLoggedObjectStore ObjectStore { get; }
    public Table(IIndexedLoggedObjectStore objectStore)
    {
        ObjectStore = objectStore;
    }

    public async Task<string> BeginTransaction()
    {
        return await ObjectStore.BeginTransaction();
    }

    public async Task<bool> CommitTransaction(string TransactionId)
    {
        return await ObjectStore.CommitTransaction(TransactionId);
    } 

    public async Task<bool> CancelTransaction(string TransactionId)
    {
        return await ObjectStore.CancelTransaction(TransactionId);
    } 

    public async Task Delete(string rowKey, int transactionId)
    {
        await ObjectStore.Delete(transactionId, rowKey);
    }
    public async Task Upsert(string rowKey, T value, int transactionId)
    {
        await ObjectStore.Upsert(transactionId, rowKey, JsonSerializer.Serialize(value));
    }
    public async Task<T> Read(string rowKey)
    {
        return JsonSerializer.Deserialize<T>(await ObjectStore.Read(rowKey));
    }
    public async Task<IEnumerable<T>> Query(string text)
    {
        return await Task.WhenAll((await ObjectStore.Query(WhereExpressionComparison.ParseExpressionText<T>(text))).Select(DeserializeFromBytesAsync));
    }
    public static async Task<T> DeserializeFromBytesAsync(byte[] serializedObject)
    {   
        using var stream = new MemoryStream(serializedObject);
        return await JsonSerializer.DeserializeAsync<T>(stream);
    }
}
