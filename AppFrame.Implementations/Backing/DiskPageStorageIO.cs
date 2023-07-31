using AppFrame.Interfaces;
namespace AppFrame.Implementations.PageStorage
{
    public class DiskBackedPageStorageIO : IPageStorageIO
    {    
        readonly string Location;
        public DiskBackedPageStorageIO(string _Location)
        {
            this.Location = _Location;
            Directory.CreateDirectory(Location);
        }
        public async Task<byte[]> GetPageAsync(string fileId, int pageNumber)
        {
            // byte[] pageData = ;
            // using FileStream fileStream = File.OpenRead(GetFilePath(fileId, pageNumber));
            // fileStream.Seek(pageNumber * pageSizeBytes, SeekOrigin.Begin);
            // await fileStream.ReadAsync(pageData.AsMemory(0, pageSizeBytes));
            return await File.ReadAllBytesAsync(GetFilePath(fileId, pageNumber));
        }
        public async Task<Stream> GetPageStreamAsync(string fileId, int pageNumber)
        {
            using FileStream fileStream = File.OpenRead(GetFilePath(fileId, pageNumber));
            fileStream.Seek(0, SeekOrigin.Begin);
            MemoryStream memoryStream = new();
            await fileStream.CopyToAsync(memoryStream);
            fileStream.Close();
            memoryStream.Position = 0;
            return memoryStream;
        }
        public async Task SetPageAsync(string fileId, int pageNumber, byte[] data)
        {
            await File.WriteAllBytesAsync(GetFilePath(fileId, pageNumber), data);
        }
        public async Task SetPageStreamAsync(string fileId, int pageNumber, Stream data)
        {
            using FileStream fileStream = File.OpenWrite(GetFilePath(fileId, pageNumber));
            fileStream.Seek(0, SeekOrigin.Begin);
            await data.CopyToAsync(fileStream);
        }
        public Task DeletePage(string fileId, int pageNumber)
        {
           File.Delete(GetFilePath(fileId, pageNumber));
           return Task.CompletedTask;        
        }
        private string GetFilePath(string fileId, int pageNumber)
        {
            return Path.Combine(Location, $"{fileId}-{pageNumber}.page");
        }
    }
}