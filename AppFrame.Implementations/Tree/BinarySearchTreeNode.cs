using System.Text;
using AppFrame.Interfaces;

namespace AppFrame.Implementations;

public class BinarySearchTreeNode : ISearchTreeNode
{
    private readonly BinarySearchTree _tree;

    public BinarySearchTreeNode(BinarySearchTree tree, byte[]? key, string? reference = null)
    {
        _tree = tree;
        Key = key;
       Reference = reference;
    }
    public BinarySearchTreeNode(BinarySearchTree tree, byte[]? key, string? reference = null, byte[]? parentKey = null, byte[]? leftChildKey = null, byte[]? rightChildKey = null)
    {
        _tree = tree;
        Key = key;
        Reference = reference;
        _parentKey = parentKey;
        _rightChildKey = rightChildKey;
        _leftChildKey = leftChildKey;
    }
    public static BinarySearchTreeNode DeserializeBytes(BinarySearchTree tree, byte[] data)
    {
        int offset = 0;

        // Read the lengths of the arrays
        byte[]? Key = ReadBytesAndPopOffsetBytes(ref offset, data);
        byte[]? _parentKey = ReadBytesAndPopOffsetBytes(ref offset, data);
        byte[]? _leftChildKey = ReadBytesAndPopOffsetBytes(ref offset, data);
        byte[]? _rightChildKey = ReadBytesAndPopOffsetBytes(ref offset, data);
        string? Reference = ReadBytesAndPopOffsetString(ref offset, data);

        return new BinarySearchTreeNode (
            tree,
            Key,
            Reference,
            _parentKey,
            _leftChildKey,
            _rightChildKey
        );

    }
    public static byte[] SerializeBytes(BinarySearchTreeNode node)
    {
        int keyLength = node.Key?.Length ?? 0;
        int referenceLength = Encoding.UTF8.GetByteCount(node?.Reference ?? String.Empty);
        int parentKeyLength = node._parentKey?.Length ?? 0;
        int leftChildKeyLength = node._leftChildKey?.Length ?? 0;
        int rightChildKeyLength = node._rightChildKey?.Length ?? 0;

        int bufferLength = 20 + keyLength + referenceLength + parentKeyLength + leftChildKeyLength + rightChildKeyLength;
        byte[] data = new byte[bufferLength];
        int offset = 0;

        // Write the lengths of the arrays
        WriteBytesAndPushOffsetBytes(node.Key, ref offset, ref data);
        WriteBytesAndPushOffsetBytes(node._parentKey, ref offset, ref data);
        WriteBytesAndPushOffsetBytes(node._leftChildKey, ref offset, ref data);
        WriteBytesAndPushOffsetBytes(node._rightChildKey, ref offset, ref data);
        WriteBytesAndPushOffsetString(node.Reference, ref offset, ref data);
        
        return data;
    }
    private static string ReadBytesAndPopOffsetString(ref int offset, byte[] buffer)
    {
        // Read the length integer from the buffer
        int length = BitConverter.ToInt32(buffer, offset);
        offset += 4;

        // Read the actual data
        string data = Encoding.UTF8.GetString(buffer, offset, length);
        offset += length;

        return data;
    }
    private static byte[] ReadBytesAndPopOffsetBytes(ref int offset, byte[] buffer)
    {
        // Read the length integer from the buffer
        int length = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        byte[] data = new byte[length];

        // Read the actual data
        Buffer.BlockCopy(buffer, offset, data, 0, length);
        offset += length;

        return data;
    }
    private static void WriteBytesAndPushOffsetString(string? data, ref int offset, ref byte[] buffer)
    {
        // Encode the string 
        byte[] stringBytes = Encoding.UTF8.GetBytes(data ?? String.Empty);

        // Write the length integer to the buffer
        Buffer.BlockCopy(BitConverter.GetBytes(stringBytes.Length), 0, buffer, offset, 4);
        offset += 4;

        // Now copy the actual data 
        Buffer.BlockCopy(stringBytes, 0, buffer, offset, stringBytes.Length);
        offset += stringBytes.Length;

    }
    private static void WriteBytesAndPushOffsetBytes(byte[]? data, ref int offset, ref byte[] buffer)
    {
        data ??= Array.Empty<byte>();

        // Write the length integer to the buffer
        Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, buffer, offset, 4);
        offset += 4;

        // now copy the actual data 
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;

    }

    public byte[]? Key { get; set; }
    public string? Reference { get; set; }

    private byte[]? _parentKey;
    private byte[]? _leftChildKey;
    private byte[]? _rightChildKey;

    private ISearchTreeNode _parent;
    private ISearchTreeNode _leftChild;
    private ISearchTreeNode _rightChild;

    public ISearchTreeNode Parent
    {
        get
        {
            _parent ??= _tree.LoadNodeFromStoreByReferenceKey(_parentKey).Result;
            return _parent;
        }
        set
        {
            _parent = value;
            _parentKey = value?.Parent?.Key ?? Array.Empty<byte>();
        }
    }

    public ISearchTreeNode LeftChild
    {
        get
        {
            _leftChild ??= _tree.LoadNodeFromStoreByReferenceKey(_leftChildKey).Result;
            return _leftChild;
        }
        set
        {
            _leftChild = value;
            _leftChildKey = value?.LeftChild.Key ?? Array.Empty<byte>();
        }
    }

    public ISearchTreeNode RightChild
    {
        get
        {
            _rightChild ??= _tree.LoadNodeFromStoreByReferenceKey(_rightChildKey).Result;
            return _rightChild;
        }
        set
        {
            _rightChild = value;
            _rightChildKey = value?.RightChild.Key ?? Array.Empty<byte>();
        }
    }
}

// public class BinarySearchTreeNode<TKey, TValue> : ISearchTreeNode<TKey, TValue>
// {
//     private readonly BinarySearchTree<TKey, TValue> _tree;

//     public BinarySearchTreeNode(BinarySearchTree<TKey, TValue> tree, TKey key, TValue reference = default)
//     {
//         _tree = tree;
//         Key = key;
//         Reference = reference;
//     }

//     public static BinarySearchTreeNode<TKey, TValue> DeserializeBytes(BinarySearchTree<TKey, TValue> tree, byte[] data)
//     {
//         int offset = 0;

//         TKey key = ReadBytesAndPopOffset<TKey>(ref offset, data);
//         TValue reference = ReadBytesAndPopOffset<TValue>(ref offset, data);

//         return new BinarySearchTreeNode<TKey, TValue>(tree, key, reference);
//     }

//     public static byte[] SerializeBytes(BinarySearchTreeNode<TKey, TValue> node)
//     {
//         int keyLength = node.Key is null ? 0 : BinarySizeHelper.SizeOf(node.Key);
//         int referenceLength = BinarySizeHelper.SizeOf(node.Reference);

//         int bufferLength = 8 + keyLength + referenceLength;
//         byte[] data = new byte[bufferLength];
//         int offset = 0;

//         WriteBytesAndPushOffset(node.Key, ref offset, ref data);
//         WriteBytesAndPushOffset(node.Reference, ref offset, ref data);

//         return data;
//     }

//     private static T ReadBytesAndPopOffset<T>(ref int offset, byte[] buffer)
//     {
//         int size = BinarySizeHelper.SizeOf<T>();

//         T value = BinaryConverter.FromBytes<T>(buffer, offset);
//         offset += size;

//         return value;
//     }

//     private static void WriteBytesAndPushOffset<T>(T value, ref int offset, ref byte[] buffer)
//     {
//         byte[] bytes = BinaryConverter.ToBytes(value);
//         Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
//         offset += bytes.Length;
//     }

//     public TKey Key { get; set; }
//     public TValue Reference { get; set; }
//      private byte[]? _parentKey;
//     private byte[]? _leftChildKey;
//     private byte[]? _rightChildKey;

//     private ISearchTreeNode _parent;
//     private ISearchTreeNode _leftChild;
//     private ISearchTreeNode _rightChild;

//     public ISearchTreeNode Parent
//     {
//         get
//         {
//             _parent ??= _tree.LoadNodeFromStoreByReferenceKey(_parentKey).Result;
//             return _parent;
//         }
//         set
//         {
//             _parent = value;
//             _parentKey = value?.Parent?.Key ?? Array.Empty<byte>();
//         }
//     }

//     public ISearchTreeNode LeftChild
//     {
//         get
//         {
//             _leftChild ??= _tree.LoadNodeFromStoreByReferenceKey(_leftChildKey).Result;
//             return _leftChild;
//         }
//         set
//         {
//             _leftChild = value;
//             _leftChildKey = value?.LeftChild.Key ?? Array.Empty<byte>();
//         }
//     }

//     public ISearchTreeNode RightChild
//     {
//         get
//         {
//             _rightChild ??= _tree.LoadNodeFromStoreByReferenceKey(_rightChildKey).Result;
//             return _rightChild;
//         }
//         set
//         {
//             _rightChild = value;
//             _rightChildKey = value?.RightChild.Key ?? Array.Empty<byte>();
//         }
//     }
// }
// public static class BinarySizeHelper
// {
//     public static int SizeOf<T>()
//     {
//         Type type = typeof(T);

//         if (type == typeof(bool))
//         {
//             return sizeof(bool);
//         }
//         else if (type == typeof(byte))
//         {
//             return sizeof(byte);
//         }
//         else if (type == typeof(char))
//         {
//             return sizeof(char);
//         }
//         else if (type == typeof(short))
//         {
//             return sizeof(short);
//         }
//         else if (type == typeof(int))
//         {
//             return sizeof(int);
//         }
//         else if (type == typeof(long))
//         {
//             return sizeof(long);
//         }
//         else if (type == typeof(float))
//         {
//             return sizeof(float);
//         }
//         else if (type == typeof(double))
//         {
//             return sizeof(double);
//         }
//         else
//         {
//             // For other types, you can define custom logic based on your requirements
//             // For example, if you have a fixed-size struct, you can return the size of that struct
//             // If you have a variable-size type like string, you may need to handle it differently
//             // You can also consider using attributes or configuration to specify the size of custom types
//             throw new NotSupportedException($"SizeOf is not supported for type {type.Name}.");
//         }
//     }
// }