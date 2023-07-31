using AppFrame.Interfaces;
using AppFrame.Expressions;
using System.Text.Json;
using System.Text;
namespace AppFrame.Implementations;
public class IndexedLoggedObjectStore : IIndexedLoggedObjectStore
{
    // At this level of abstraction we are dealing with the following:
    // Indexing of the different key values for the KV Store (this is done by the search trees)
    // Transaction Logged KV Storage (this is done by the transaction log and the paged storage manager) via the LoggedKeyValueStore

    public ILoggedKeyValueStore lkvs { get; }
    public Dictionary<string, ISearchTree> searchTrees { get; }
    public IndexedLoggedObjectStore(ILoggedKeyValueStore lkvs, Dictionary<string, ISearchTree> searchTrees)
    {
        this.lkvs = lkvs;
        this.searchTrees = searchTrees;
    }

    public async Task<string> BeginTransaction(){

        return await lkvs.BeginTransaction();

    }
    public async Task<bool> CommitTransaction(string TransactionId){

        return await lkvs.SetTransactionState(TransactionId, CommitState.Committed);

    }
    public async Task<bool> CancelTransaction(string TransactionId){

        return await lkvs.SetTransactionState(TransactionId, CommitState.Broken);

    }
    public async Task Delete(int transactionId, string key)
    {
        // Blow away the main store by using the key reference 
        await lkvs.Delete(transactionId, key);
        await ExtractIndexKeyFromDiskObjectAndRemoveFromIndexes(transactionId, key);
    }
    private async Task ExtractIndexKeyFromJsonDocumentAndAddToIndexes(int transactionId, string Document, string key)
    {
        JsonDocument doc = JsonDocument.Parse(Document);
        foreach (JsonProperty property in doc.RootElement.EnumerateObject().Where(p => searchTrees.ContainsKey(p.Name.ToLower())))
        {
            byte[] encodedBytes = GetEncodedBytesFromJsonProperty(property);
            await searchTrees[property.Name.ToLower()].Insert(encodedBytes, key, transactionId);
        }
    }
    private async Task ExtractIndexKeyFromDiskObjectAndRemoveFromIndexes(int transactionId, string key)
    {
        JsonDocument jsonDoc = JsonDocument.Parse(await Read(key));

        foreach (JsonProperty property in jsonDoc.RootElement.EnumerateObject().Where(p => searchTrees.ContainsKey(p.Name.ToLower())))
        {
            byte[] encodedBytes = GetEncodedBytesFromJsonProperty(property);
            await searchTrees[property.Name.ToLower()].Remove(encodedBytes, transactionId);
        }
    }
    private static byte[] GetEncodedBytesFromJsonProperty(JsonProperty property)
    {
        byte[] encodedBytes = Array.Empty<byte>();
        switch (property.Value.ValueKind)
        {
            case JsonValueKind.Object:
                encodedBytes = Encoding.UTF8.GetBytes(property.Value.GetRawText());
                break;
            case JsonValueKind.Null: 
                break;
            case JsonValueKind.True:
                encodedBytes = BitConverter.GetBytes(property.Value.GetBoolean());   
                break;
            case JsonValueKind.False:
                encodedBytes = BitConverter.GetBytes(property.Value.GetBoolean());
                break;
            case JsonValueKind.String:
                encodedBytes = Encoding.UTF8.GetBytes(property.Value.GetString());
                break;
            case JsonValueKind.Array:
                encodedBytes = property.Value.GetBytesFromBase64();
                break;
            case JsonValueKind.Number:

                if (property.Value.TryGetInt32(out int intValue))
                {
                    encodedBytes = BitConverter.GetBytes(intValue);
                }
                else if (property.Value.TryGetDouble(out double doubleValue))
                {
                    encodedBytes = BitConverter.GetBytes(doubleValue);
                }
                else if (property.Value.TryGetDecimal(out decimal decimalValue))
                {
                    encodedBytes = Decimal.GetBits(decimalValue)
                        .SelectMany(BitConverter.GetBytes)
                        .ToArray();
                }
                break;
            default:
                byte[] bytes = property.Value.GetBytesFromBase64();
                break;
        }
        return encodedBytes;
    }
    public async Task<bool> Exists(string key)
    {
        return await lkvs.Exists(key);
    }
    public async Task<string> Read(string key)
    {
        return Encoding.UTF8.GetString(await lkvs.Read(key));
    }
    public async Task Upsert(int transactionId, string key, string value)
    {
        bool update = await lkvs.Exists(key);  
        await lkvs.Upsert(transactionId, key, await SerializeToUTF8Bytes(value));
        if(update) await ExtractIndexKeyFromDiskObjectAndRemoveFromIndexes(transactionId, key);
        await ExtractIndexKeyFromJsonDocumentAndAddToIndexes(transactionId, value, key);
    }
    private static Task<byte[]> SerializeToUTF8Bytes<T>(T value)
    {
        return Task.FromResult(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));
    }
    private static Task<T> DeserializeToT<T>(byte[] utf8bytes)
    {
        return Task.FromResult(JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(utf8bytes)));
    }
    public async Task<IEnumerable<byte[]>> Query(IEnumerable<WhereExpressionComparison> expressions)
    {
        // Search all of the relevant indexes (integer keys) and return the list of objects back from storage 
    
        // Theory wise, running an indexed query against a * single table * should go as follows
        // 1. WhereExpression tells us which records in the tree to return the references for
        Dictionary<string, IEnumerable<byte[]>> finalResults = new ();

        foreach(WhereExpressionComparison expression in expressions)
        {
            foreach (KeyValuePair<string, ISearchTree> tree in this.searchTrees.Where(e => e.Key == expression.FieldName))
            {
                // if so, SEARCH THE TREE with the correct expression AND ADD THE result to 'finalResults'
                // create the correct predicate
                Func<byte[], bool> pred = BinaryHelpers.GetPredicate(expression.Operator, expression.Value);
                IEnumerable<byte[]> matchingKeys = await tree.Value.SearchKeys(pred);
                finalResults.Add(tree.Key, matchingKeys);
            }
        }

        // 2. Union the finalResults sets of results to see if the resulting tree index entries are in ALL of the sets returned
        IEnumerable<byte[]> resultKeys = finalResults.Values.Aggregate((previous, current) => previous.Intersect(current).ToList());

        // 3. return the list of matching record keys, where the integer key is present in all executed queries of indexes
        return resultKeys;
    }
}
