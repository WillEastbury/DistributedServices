using System.IO.Compression;
using System;
using AppFrame.Paging;

namespace AppFrame.Utilities;
public static class Utilities
{
    public static int PageSize {get;set;} = 31744; // 31k



    public static byte[] Decompress(byte[] compressedData)
    {
        using MemoryStream memoryStream = new(compressedData);
        using DeflateStream deflateStream = new(memoryStream, CompressionMode.Decompress);
        using MemoryStream decompressedMemoryStream = new();
        deflateStream.CopyTo(decompressedMemoryStream);
        return decompressedMemoryStream.ToArray();
    }

    public static byte[] Compress(byte[] data)
    {
        using MemoryStream memoryStream = new();
        using DeflateStream deflateStream = new(memoryStream, CompressionMode.Compress);
        deflateStream.Write(data, 0, data.Length);
        deflateStream.Flush();
        return memoryStream.ToArray();
    }

    public static async Task<byte[]> DecompressAsync(byte[] compressedData)
    {
        using MemoryStream memoryStream = new(compressedData);
        using DeflateStream deflateStream = new(memoryStream, CompressionMode.Decompress);
        using MemoryStream decompressedMemoryStream = new();
        await deflateStream.CopyToAsync(decompressedMemoryStream);
        return decompressedMemoryStream.ToArray();
    }

    public static async Task<byte[]> CompressAsync(byte[] data)
    {
        using MemoryStream memoryStream = new();
        using DeflateStream deflateStream = new(memoryStream, CompressionMode.Compress);
        await deflateStream.WriteAsync(data);
        await deflateStream.FlushAsync();
        return memoryStream.ToArray();
    }

    public static Task<IEnumerable<byte[]>> SplitIntoDataPages(byte[] data)
    {
        // Split the data into pages of the requested size, if data is null then return an empty list.
        List<byte[]> dataPages = new();
        int offset = 0;
        while (offset < (data?.Length ?? 0))
        {
            int length = Math.Min(PageSize, data.Length - offset);
            byte[] dataPage = new byte[length];
            Buffer.BlockCopy(data, offset, dataPage, 0, length);
            dataPages.Add(dataPage);
            offset += length;
        }
        return Task.FromResult(dataPages.AsEnumerable());
    }
    
    public static Task<IEnumerable<PageChange>> GetPageSetChanges(IEnumerable<byte[]> oldPages, IEnumerable<byte[]> newPages)
    {
        // Loop through and compare the pages in the old and new sets, return an enumerable of differences between pagesets that we need to record for the diff
        // If the old page set is null, then we need to record all of the new pages as new pages.
        // If the new page set is null, then we need to record all of the old pages as deleted pages.
        // If the old page set is not null, and the new page set is not null, then we need to compare the two sets and record the differences.
        List<PageChange> pageChanges = new();

        if (oldPages is not null && newPages is not null)
        {
            int pageNumber = 0;
            var oldEnumerator = oldPages.GetEnumerator();
            var newEnumerator = newPages.GetEnumerator();

            bool oldEnumeratorHasNext = oldEnumerator.MoveNext();
            bool newEnumeratorHasNext = newEnumerator.MoveNext();

            while (oldEnumeratorHasNext || newEnumeratorHasNext)
            {
                byte[] oldPage = oldEnumeratorHasNext ? oldEnumerator.Current : null;
                byte[] newPage = newEnumeratorHasNext ? newEnumerator.Current : null;

                if (AreByteArraysEqual(oldPage, newPage))
                {
                    // Pages are identical, no change recorded
                }
                else if (oldPage is null && newPage is not null)
                {
                    // New page added
                    pageChanges.Add(new PageChange(pageNumber, null, newPage, PageChangeState.New));
                }
                else if (oldPage is not null && newPage is null)
                {
                    // Page deleted
                    pageChanges.Add(new PageChange(pageNumber, oldPage, null, PageChangeState.Deleted));
                }
                else
                {
                    // Page updated
                    pageChanges.Add(new PageChange(pageNumber, oldPage, newPage, PageChangeState.Updated));
                }

                pageNumber++;
                oldEnumeratorHasNext = oldEnumerator.MoveNext();
                newEnumeratorHasNext = newEnumerator.MoveNext();
            }
        }

        return Task.FromResult(pageChanges.AsEnumerable());
    }

    public static bool AreByteArraysEqual(byte[] array1, byte[] array2)
    {
        if (array1 is null && array2 is null)
            return true;

        if (array1 != null || array2 == null)
            return false;

        if (array1 == null || array2 != null)
            return false;

        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
                return false;
        }
        return true;
    }
}
