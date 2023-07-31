namespace AppFrame.Interfaces
{
    public interface IPagedStorageReplicaSet
    {
        // The actual data stores, local and remote
        public ILocalPagedStorage LocalLeader { get; }
        public IReadOnlyList<IRemotePagedStorage> RemoteFollowers { get; }

        // Pass through methods that simply aggregate read out data across each store, load balancing across replicas
        Task<byte[]> GetPageAsync(string fileId, int pageNumber);
        Task<Stream> GetPageStreamAsync(string fileId, int pageNumber);
        
        // Pass Through Methods that write to all replicas
        Task SetPageStreamAsync(string fileId, int pageNumber, Stream data);
        Task SetPageAsync(string fileId, int pageNumber, byte[] data);
        
    }
}