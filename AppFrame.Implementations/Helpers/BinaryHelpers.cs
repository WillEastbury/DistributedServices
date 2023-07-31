using AppFrame.Expressions;
namespace AppFrame.Implementations;

public static class BinaryHelpers
{
    public const byte EscapeChar = (byte)'\\';
    public const byte Delimiter = 0xFF;
    public static Func<byte[], bool> GetPredicate(QueryOperator qoperator, byte[] valuetomatch)
    {
        Func<byte[], bool> pred = qoperator switch
        {
            QueryOperator.Equals => (key) => key.SequenceEqual(valuetomatch),
            QueryOperator.NotEquals => (key) => !key.SequenceEqual(valuetomatch),
            QueryOperator.GreaterThan => (key) => Compare(key, valuetomatch) > 0,
            QueryOperator.LessThan => (key) => Compare(key, valuetomatch) < 0,
            QueryOperator.GreaterThanOrEqual => (key) => Compare(key, valuetomatch) >= 0,
            QueryOperator.LessThanOrEqual => (key) => Compare(key, valuetomatch) <= 0,
            QueryOperator.Contains => (key) => Contains(key, valuetomatch),
            QueryOperator.StartsWith => (key) => StartsWith(key, valuetomatch),
            QueryOperator.EndsWith => (key) => EndsWith(key, valuetomatch),
            QueryOperator.IsNull => (key) => key == null,
            QueryOperator.IsNotNull => (key) => key != null,
            _ => throw new Exception($"Invalid QueryOperator: {qoperator}"),
        };

        return pred;
    }

    public static byte[] Escape(IEnumerable<byte[]> byteArrays)
    {
        List<byte> escapedBytes = new();
        foreach (var byteArray in byteArrays)
        {
            foreach (var b in byteArray)
            {
                if (b == EscapeChar || b == Delimiter)
                {
                    escapedBytes.Add(EscapeChar);
                }
                escapedBytes.Add(b);
            }
            escapedBytes.Add(Delimiter);
        }

        return escapedBytes.ToArray();
    }

    public static List<byte[]> Unescape(byte[] escapedBytes)
    {
        List<byte[]> byteArrays = new();
        List<byte> byteArray = new();

        for (int i = 0; i < escapedBytes.Length; i++)
        {
            byte b = escapedBytes[i];

            if (b == EscapeChar)
            {
                i++; // Skip the escape character and take the next byte as is
                byte nextByte = escapedBytes[i];
                byteArray.Add(nextByte);
            }
            else if (b == Delimiter)
            {
                byteArrays.Add(byteArray.ToArray());
                byteArray.Clear();
            }
            else
            {
                byteArray.Add(b);
            }
        }

        return byteArrays;
    }
    
    // Implementation of custom comparison methods for byte[]
    public static bool Contains(byte[] source, byte[] pattern)
    {
        for (int i = 0; i < source.Length - pattern.Length + 1; i++)
        {
            if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
            {
                return true;
            }
        }
        return false;
    }

    public static bool StartsWith(byte[] source, byte[] pattern)
    {
        if (source.Length < pattern.Length)
            return false;

        return source.Take(pattern.Length).SequenceEqual(pattern);
    }

    public static bool EndsWith(byte[] source, byte[] pattern)
    {
        if (source.Length < pattern.Length)
            return false;

        return source.Skip(source.Length - pattern.Length).SequenceEqual(pattern);
    }

    public static int Compare(byte[] left, byte[] right)
    {
        int minLength = Math.Min(left.Length, right.Length);

        for (int i = 0; i < minLength; i++)
        {
            int comparison = left[i].CompareTo(right[i]);
            if (comparison != 0)
                return comparison;
        }

        return left.Length.CompareTo(right.Length);

    }
}