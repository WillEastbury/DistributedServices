using System.Collections.Concurrent;
using AppFrame.Interfaces;
namespace AppFrame.Implementations;
public class InMemoryCache : ICache
{
    private ConcurrentDictionary<string, object> cache = new();
    private ConcurrentDictionary<string, DateTime> expirationTimes = new();
    readonly Task CacheManagerTask;
    public InMemoryCache()
    {
        CacheManagerTask = CacheManager();
        CacheManagerTask.Start();
    }
    public void Set<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        if (expirationTime == null) expirationTime = TimeSpan.FromHours(1);
        DateTime expiry = DateTime.Now + expirationTime.Value;
        cache.AddOrUpdate(key, value, (k, v) => value);
        expirationTimes.AddOrUpdate(key, expiry, (k, v) => expiry);
    }
    public T Get<T>(string key)
    {
        return (T)cache[key];
    }
    public void Remove(string key)
    {
        cache.TryRemove(key, out _);
        expirationTimes.TryRemove(key, out _);
    }
    public void Clear()
    {
        cache = new();
        expirationTimes = new();
    }
    public async Task CacheManager()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                foreach (var key in expirationTimes.Keys)
                {
                    if (expirationTimes[key] < DateTime.Now)
                    {
                        Remove(key);
                    }
                }
            }
        });
    }
}