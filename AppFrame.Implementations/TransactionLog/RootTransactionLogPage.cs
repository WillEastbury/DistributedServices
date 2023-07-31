namespace AppFrame.Implementations.TransactionLog;

public struct RootTransactionLogPage
{
    public RootTransactionLogPage()
    {
        
    }

    public int LogLengthInPages { get; set; } = 0;
    public int LogStartPageNumber { get; set; } = 0;
    public int LogEndPageNumber { get; set; } = 0;
    
    public int NextTransactionId = 0;
    public bool Dirty {get;set;} = false;
    public int GetNextTransactionId() 
    {
        return Interlocked.Increment(ref NextTransactionId);
    }
}
