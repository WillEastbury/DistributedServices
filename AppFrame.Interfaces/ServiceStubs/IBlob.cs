namespace AppFrame.Interfaces;
public interface IBlob
{
    ITable<Blob> table { get; }
    Task<byte[]> Snapshot(string key, string etag);
    Task Delete(string key, string etag, string transactionId = null);
    Task<byte[]> Read(string key, string snapshotId = null);
    Task<string> Create(string key, byte[] value, string etag, string transactionId = null);
    Task<string> Update(string key, byte[] value, string etag, string transactionId = null);
    Task<string> Patch(string key, PatchInstruction[] patchInstructions, string etag, string transactionId = null);

    // Stream-based versions of Read,Create,Update,Patch
    Task<Stream> ReadStream(string key, string snapshotId = null);
    Task<string> CreateStream(string key, Stream value, string etag, string transactionId = null);
    Task<string> UpdateStream(string key, Stream value, string etag, string transactionId = null);
    Task<string> PatchStream(string key, PatchInstruction[] patchInstructions, string etag, string transactionId = null);
    Task<string> Lease(string key, string etag);
}
public class Blob
{
    public int key { get; set; }
    public string path { get; set; }
    public int size { get; set; } = 0;
    public DateTime created {get;set;} = DateTime.UtcNow;
    public DateTime updated {get;set;} = DateTime.UtcNow;
    public string etag {get;set;} = "";
    public byte[] MD5 {get;set;} = null;
    public Dictionary<DateTime, string> snapshots {get;set;} = new();
    public byte[] Data {get;set;} = Array.Empty<byte>();
}