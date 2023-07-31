using AppFrame.Interfaces;

namespace AppFrame.Implementations.PageStorage
{
    public class LocalPagedStorage : ILocalPagedStorage
    {
        public IPageStorageIO LocalStorage { get; }
        public ICache Cache { get; }
        public TimeSpan DefaultCacheTimeSpan { get; } = TimeSpan.FromMinutes(30);
        public LocalPagedStorage(IPageStorageIO localStorage, ICache cache, TimeSpan? DefaultCacheTimeSpan)
        {
            LocalStorage = localStorage;
            Cache = cache;
            if (DefaultCacheTimeSpan.HasValue) this.DefaultCacheTimeSpan = DefaultCacheTimeSpan.Value;
        }
        public async Task<byte[]> GetPageAsync(string fileId, int pageNumber)
        {
            string cacheKey = GetCacheKey(fileId, pageNumber);
            byte[] pageData = Cache.Get<byte[]>(cacheKey);
            if (pageData == null)
            {
                pageData = await LocalStorage.GetPageAsync(fileId, pageNumber);
                Cache.Set(cacheKey, pageData, DefaultCacheTimeSpan);
            }
            return pageData;
        }
        public async Task SetPageAsync(string fileId, int pageNumber, byte[] data)
        {
            await LocalStorage.SetPageAsync(fileId, pageNumber, data);
            Cache.Set(GetCacheKey(fileId, pageNumber), data, DefaultCacheTimeSpan);
        }
        public async Task DeletePageAsync(string fileId, int pageNumber)
        {
            await LocalStorage.DeletePage(fileId, pageNumber);
            Cache.Remove(GetCacheKey(fileId, pageNumber));
        }
        public async Task<Stream> GetPageStreamAsync(string fileId, int pageNumber)
        {
            string cacheKey = GetCacheKey(fileId, pageNumber);           
            byte[] pageData = Cache.Get<byte[]>(cacheKey);
            if (pageData == null)
            {
                using Stream sourceStream = await LocalStorage.GetPageStreamAsync(fileId, pageNumber);
                using MemoryStream memoryStream = new();
                await sourceStream.CopyToAsync(memoryStream);
                pageData = memoryStream.ToArray();
                Cache.Set(cacheKey, pageData, DefaultCacheTimeSpan);
            }
            return new MemoryStream(pageData);
        }
        public async Task SetPageStreamAsync(string fileId, int pageNumber, Stream data)
        {
            string cacheKey = GetCacheKey(fileId, pageNumber);
            await LocalStorage.SetPageStreamAsync(fileId, pageNumber, data);
            Cache.Set(cacheKey, data, DefaultCacheTimeSpan);
        }
        private static string GetCacheKey(string fileId, int pageNumber)
        {
            return $"Page_{fileId}_{pageNumber}";
        }
    }
}