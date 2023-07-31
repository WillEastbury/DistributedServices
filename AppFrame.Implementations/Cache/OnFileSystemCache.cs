using System.IO;
using System.Text.Json;
using AppFrame.Interfaces;
namespace AppFrame.Implementations;
public class OnFileSystemCache : ICache
{
    readonly string Location;
    public OnFileSystemCache(string _Location)
    {
        this.Location = _Location;
        Directory.CreateDirectory(Location);
    }
    public void Set<T>(string key, T value, TimeSpan? expirationTime)
    {
        if (expirationTime != null) throw new NotSupportedException("The OnFileSystemCache does not support expiration times for performance reasons");
        
        File.WriteAllBytes(Path.Combine(Location, key), JsonSerializer.SerializeToUtf8Bytes<T>(value));
    }
    public T Get<T>(string key)
    {
       return JsonSerializer.Deserialize<T>(File.ReadAllBytes(Path.Combine(Location, key)));
    }
    public void Remove(string key)
    {
        File.Delete(Path.Combine(Location, key));
    }
    public void Clear()
    {
        Directory.Delete(Location, true);
        Directory.CreateDirectory(Location);
    }
}