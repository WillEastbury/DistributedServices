namespace AppFrame.Interfaces;
public interface IRequestProxy
{
    ICluster Cluster { get; set; }
    
    Task<HttpResponseMessage> ProxyToPartitionMethod(HttpRequestMessage request, string service, string partitionKey);
}
