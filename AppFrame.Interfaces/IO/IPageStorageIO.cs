namespace AppFrame.Interfaces;
public interface IPageStorageIO
{
    Task<byte[]> GetPageAsync(string fileId, int pageNumber);
    Task SetPageAsync(string fileId, int pageNumber, byte[] data);
    Task DeletePage(string fileId, int pageNumber);
    Task<Stream> GetPageStreamAsync(string fileId, int pageNumber);
    Task SetPageStreamAsync(string fileId, int pageNumber, Stream data);
}
