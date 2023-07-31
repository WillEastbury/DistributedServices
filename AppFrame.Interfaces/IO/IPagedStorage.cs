using System.Threading.Tasks;
namespace AppFrame.Interfaces
{
    public interface ILocalPagedStorage
    {
        IPageStorageIO LocalStorage { get; }
        ICache Cache { get; }
        Task<byte[]> GetPageAsync(string fileId, int pageNumber);
        Task SetPageAsync(string fileId, int pageNumber, byte[] data);
        Task DeletePageAsync(string fileId, int pageNumber);
        Task<Stream> GetPageStreamAsync(string fileId, int pageNumber);
        Task SetPageStreamAsync(string fileId, int pageNumber, Stream data);
    }

    public interface IRemotePagedStorage
    {
        Task<Stream> GetRemotePageStreamAsync(string fileId, int pageNumber);
        Task SetRemotePageStreamAsync(string fileId, int pageNumber, Stream data);
        Task DeletePageAsync(string fileId, int pageNumber);
        Task<byte[]> GetRemotePageAsync(string fileId, int pageNumber);
        Task SetRemotePageAsync(string fileId, int pageNumber, byte[] data);
    }
}