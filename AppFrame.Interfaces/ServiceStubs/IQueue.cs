namespace AppFrame.Interfaces;
public interface IQueue
{
    ITable<Queue> tableQs { get; } // Dependency on IBasicBlobStore
    ITable<QueueMessage> tableQMessages{ get; } // Dependency on IBasicBlobStore
    Task<string> Enqueue(string queueName, byte[] message, string SessionId);
    Task<byte[]> Dequeue(string queueName, string SessionId);
    Task<bool> Lease(string queueName, string messageId, int leaseDuration);
    Task<bool> Extend(string queueName, string messageId, int leaseDuration);
    Task Flush(string queueName);
    Task<bool> Fail(string queueName, string messageId);
}
public class Queue
{
    public string QueueName { get; set; }
    public int MaxMessages { get; set; }
    public int MaxSize {get;set;}
    public int CurrentMessages {get;set;}
    public int DeadLetterAttempts {get;set;} = 10;
}
public class QueueMessage
{
    public string QueueName{ get; set; }
    public string SessionId { get; set; }
    public byte[] Message{ get; set; }
    public DateTime EnqueuedInUTC { get; set; }
}