namespace AppFrame.Interfaces;
public interface ILock
{
    ITable<Lock> table { get; set; }
    Task<bool> Unlock(string lockKey, string transactionId);
    Task<bool> RaceLock(string lockKey, string transactionId, int timeoutMs);
}
public class Lock
{
    public string LockKey {get;set;}
    public string TransactionId {get;set;}
    public DateTime UTCLockStarted {get;set;}
    public string LockedBy {get;set;}

}