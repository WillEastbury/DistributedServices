namespace AppFrame.Interfaces;
public interface ISearchTreeNode
{
    byte[] Key { get; set; }
    string? Reference { get; set; }
    ISearchTreeNode Parent { get; set; }
    ISearchTreeNode LeftChild { get; set; }
    ISearchTreeNode RightChild { get; set; }
}
public interface ISearchTreeNode<TKey, TValue>
{
    TKey Key { get; set; }
    TValue Reference { get; set; }
    ISearchTreeNode Parent { get; set; }
    ISearchTreeNode LeftChild { get; set; }
    ISearchTreeNode RightChild { get; set; }
}
