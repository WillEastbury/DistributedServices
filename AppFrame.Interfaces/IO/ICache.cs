using System.Threading.Tasks;
namespace AppFrame.Interfaces;
public interface ICache
{
    void Set<T>(string key, T value, TimeSpan? expirationTime);
    T Get<T>(string key);
    void Remove(string key);
    void Clear();
}
