using System.Linq.Expressions;
namespace AppFrame.Interfaces;

public interface ISearchTree
{
    string IndexName { get; }
    Task Insert(byte[]? key, string? reference, int transactionId);
    Task Remove(byte[]? key, int transactionId);
    Task<IEnumerable<string?>> Search(byte[] key);
    Task<IEnumerable<byte[]>> SearchKeys(Func<byte[], bool> predicate);
    Task<IEnumerable<string?>> SearchLeafValues(Func<byte[], bool> predicate);
    Task<Dictionary<byte[], string?>> Traverse();
    Task<ISearchTreeNode> LoadNodeFromStoreByReferenceKey(byte[] getKeyReference);
    Task SaveNodeToStoreViaTransaction(int TransactionId, ISearchTreeNode saveNode);

}

public interface ISearchTree<TKey, TValue>
{
    string IndexName { get; }
    Task Insert(TKey key, TValue reference, int transactionId);
    Task Remove(TKey key, int transactionId);
    Task<IEnumerable<TValue>> Search(TKey key);
    Task<IEnumerable<TKey>> SearchKeysSimpleMatch(TValue reference);
    Task<IEnumerable<TValue>> SearchKeysSimpleMatch(TKey Key);
    Task<IEnumerable<TKey>> SearchKeys(Expression<Func<TKey, bool>> expressionPredicate);
    Task<IEnumerable<TKey>> SearchKeys(Func<TKey, bool> predicate);
    Task<IEnumerable<TValue>> SearchLeafValues(Expression<Func<TKey, bool>> expressionPredicate);
    Task<IEnumerable<TValue>> SearchLeafValues(Func<TKey, bool> predicate);
    Task<Dictionary<TKey, TValue>> Traverse();
    Task<ISearchTreeNode> LoadNodeFromStoreByReferenceKey(TKey getKeyReference);
    Task SaveNodeToStoreViaTransaction(int TransactionId, ISearchTreeNode saveNode);

}


