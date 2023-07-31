using AppFrame.Interfaces;
namespace AppFrame.Implementations.PageStorage;
public class RemotePagedStorage : IRemotePagedStorage
{
    private readonly HttpClient httpClient;
    private readonly string endpointUrl;

    public RemotePagedStorage(string endpointUrl)
    {
        this.httpClient = new HttpClient();
        this.endpointUrl = endpointUrl;
    }

    public async Task<Stream> GetRemotePageStreamAsync(string fileId, int pageNumber)
    {
        string url = $"{endpointUrl}/page/{fileId}/{pageNumber}";
        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task SetRemotePageStreamAsync(string fileId, int pageNumber, Stream data)
    {
        string url = $"{endpointUrl}/page/{fileId}/{pageNumber}";
        using (var content = new StreamContent(data))
        {
            HttpResponseMessage response = await httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeletePageAsync(string fileId, int pageNumber)
    {
        string url = $"{endpointUrl}/page/{fileId}/{pageNumber}";
        HttpResponseMessage response = await httpClient.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]> GetRemotePageAsync(string fileId, int pageNumber)
    {
        string url = $"{endpointUrl}/page/{fileId}/{pageNumber}";
        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task SetRemotePageAsync(string fileId, int pageNumber, byte[] data)
    {
        string url = $"{endpointUrl}/page/{fileId}/{pageNumber}";
        using (var content = new ByteArrayContent(data))
        {
            HttpResponseMessage response = await httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}