using System.Linq.Expressions;
using AppFrame.Interfaces;
namespace AppFrame.Implementations;
public class BinarySearchTree : ISearchTree
{
    public ITransactionalPagedStorageManager itpsm { get; }
    public string IndexName { get; private set; }
    public BinarySearchTree(ITransactionalPagedStorageManager _itpsm, string indexName)
    {
        itpsm = _itpsm;
        IndexName = indexName;
    }

    public ISearchTreeNode Root { get; set; } = null;

    public async Task<ISearchTreeNode> LoadNodeFromStoreByReferenceKey(byte[] getKeyReference)
    {
        return BinarySearchTreeNode.DeserializeBytes(this, await itpsm.GetPageData(IndexName + "-" + System.Text.Encoding.UTF8.GetString(getKeyReference), 0));
    }

    public async Task SaveNodeToStoreViaTransaction(int TransactionId, ISearchTreeNode saveNode)
    {
        await itpsm.SetPageData(
            TransactionId, 
            IndexName + "-" + System.Text.Encoding.UTF8.GetString(saveNode.Key), 
            0, 
            BinarySearchTreeNode.SerializeBytes((BinarySearchTreeNode)saveNode)
        );
    }

    public async Task Insert(byte[]? key, string? reference, int transactionId)
    {
        var newNode = new BinarySearchTreeNode(this, key, reference);

        if (Root == null)
        {
            Root = newNode;
            await SaveNodeToStoreViaTransaction(transactionId, Root);
        }
        else
        {
            await InsertNodeAsync(Root, newNode, transactionId);
        }
    }

    private async Task InsertNodeAsync(ISearchTreeNode currentNode, ISearchTreeNode newNode, int transactionId)
    {
        if (currentNode == null)
            return;

        if (Comparer<byte[]>.Default.Compare(newNode.Key, currentNode.Key) < 0)
        {
            if (currentNode.LeftChild == null)
            {
                currentNode.LeftChild = newNode;
                newNode.Parent = currentNode;
                await SaveNodeToStoreViaTransaction(transactionId, newNode);
                await SaveNodeToStoreViaTransaction(transactionId, currentNode);
            }
            else
            {
                await InsertNodeAsync(currentNode.LeftChild, newNode, transactionId);
            }
        }
        else
        {
            if (currentNode.RightChild == null)
            {
                currentNode.RightChild = newNode;
                newNode.Parent = currentNode;
                await SaveNodeToStoreViaTransaction(transactionId, newNode);
                await SaveNodeToStoreViaTransaction(transactionId, currentNode);
            }
            else
            {
                await InsertNodeAsync(currentNode.RightChild, newNode, transactionId);
            }
        }
    }

    public async Task Remove(byte[]? key, int transactionId)
    {
        ISearchTreeNode nodeToRemove = (await SearchNode(key)).First();
        await RemoveNodeAsync(nodeToRemove, transactionId);
    }

    private async Task RemoveNodeAsync(ISearchTreeNode nodeToRemove, int transactionId)
    {
        if (nodeToRemove.LeftChild == null && nodeToRemove.RightChild == null)
        {
            if (nodeToRemove.Parent == null)
            {
                Root = null;
               
            }
            else if (nodeToRemove.Parent.LeftChild == nodeToRemove)
            {
                nodeToRemove.Parent.LeftChild = null;
               
            }
            else
            {
                nodeToRemove.Parent.RightChild = null;
                
            }
        }
        else if (nodeToRemove.LeftChild != null && nodeToRemove.RightChild != null)
        {
            var successor = GetMinimumNode(nodeToRemove.RightChild);
            nodeToRemove.Key = successor.Key;
            nodeToRemove.Reference = successor.Reference;
            await RemoveNodeAsync(successor, transactionId);
        }
        else
        {
            var childNode = nodeToRemove.LeftChild ?? nodeToRemove.RightChild;

            if (nodeToRemove.Parent == null)
            {
                Root = childNode;
                
            }
            else if (nodeToRemove.Parent.LeftChild == nodeToRemove)
            {
                nodeToRemove.Parent.LeftChild = childNode;
            }
            else
            {
                nodeToRemove.Parent.RightChild = childNode;
            }

            if (childNode != null)
            {
                childNode.Parent = nodeToRemove.Parent;
            }

            await SaveNodeToStoreViaTransaction(transactionId, childNode);
        }

        await SaveNodeToStoreViaTransaction(transactionId, nodeToRemove);
        await SaveNodeToStoreViaTransaction(transactionId, nodeToRemove.Parent);
  
    }

    private static ISearchTreeNode GetMinimumNode(ISearchTreeNode node)
    {
        while (node.LeftChild != null)
        {
            node = node.LeftChild;
        }

        return node;
    }

    public async Task<IEnumerable<string?>> Search(byte[] key)
    {
        var nodes = new List<ISearchTreeNode>();
        nodes.AddRange(await SearchNode(key));
        return nodes.Select(node => node.Reference);
    }
    private Task<IEnumerable<ISearchTreeNode>> SearchNode(byte[] key)
    {
        var nodes = new List<ISearchTreeNode>();
        var currentNode = Root;

        while (currentNode != null)
        {
            if (Comparer<byte[]>.Default.Compare(key, currentNode.Key) == 0)
            {
                nodes.Add(currentNode);
            }
            else if (Comparer<byte[]>.Default.Compare(key, currentNode.Key) < 0)
            {
                currentNode = currentNode.LeftChild;
            }
            else
            {
                currentNode = currentNode.RightChild;
            }
        }

        return Task.FromResult(nodes.AsEnumerable());
    }

    public async Task<IEnumerable<byte[]>> SearchKeys(Func<byte[], bool> predicate)
    {
        var keys = new List<byte[]>();
        await SearchKeys(Root, predicate, keys);
        return keys;
    }

    private async Task SearchKeys(ISearchTreeNode node, Func<byte[], bool> predicate, List<byte[]> keys)
    {
        if (node == null)
            return;

        await SearchKeys(node.LeftChild, predicate, keys);

        if (predicate(node.Key))
            keys.Add(node.Key);

        await SearchKeys(node.RightChild, predicate, keys);
    }

    public async Task<IEnumerable<string?>> SearchLeafValues(Expression<Func<byte[], bool>> expressionPredicate)
    {
        var compiledPredicate = expressionPredicate.Compile();
        var leafValues = new List<string?>();
        await SearchLeafValues(Root, compiledPredicate, leafValues);
        return leafValues;
    }

    public async Task<IEnumerable<string?>> SearchLeafValues(Func<byte[], bool> predicate)
    {
        var leafValues = new List<string?>();
        await SearchLeafValues(Root, predicate, leafValues);
        return leafValues;
    }

    private async Task SearchLeafValues(ISearchTreeNode node, Func<byte[], bool> predicate, List<string?> leafValues)
    {
        if (node == null)
            return;

        if (predicate(node.Key))
            leafValues.Add(node.Reference);

        await SearchLeafValues(node.LeftChild, predicate, leafValues);
        await SearchLeafValues(node.RightChild, predicate, leafValues);
    }

    public async Task<Dictionary<byte[], string?>> Traverse()
    {
        var result = new Dictionary<byte[], string?>();
        await TraverseInOrder(Root, result, TraversalOptions.Both);
        return result;
    }

    private async Task TraverseInOrder(ISearchTreeNode currentNode, Dictionary<byte[], string?> result, TraversalOptions traversalOptions = TraversalOptions.Both)
    {
        if (currentNode == null)
            return;

        if (traversalOptions == TraversalOptions.Left || traversalOptions == TraversalOptions.Both)
        {
            await TraverseInOrder(currentNode.LeftChild, result);
            result.Add(currentNode.Key, currentNode.Reference);
        }

        if (traversalOptions == TraversalOptions.Right || traversalOptions == TraversalOptions.Both)
        {
            await TraverseInOrder(currentNode.RightChild, result);
            result.Add(currentNode.Key, currentNode.Reference);
        }
    }

}
