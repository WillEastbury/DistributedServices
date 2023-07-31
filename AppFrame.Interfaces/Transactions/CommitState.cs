namespace AppFrame.Interfaces;

[Flags]
public enum CommitState
{
    NotCommitted = 0, // These will not have been written to the data pages (only to the log) yet and can be discarded, they may or may not have been sent to all remote replicas
    Broken = 1,       // These transactions have been written to some remote replicas but not all and cannot be committed, these should be discarded    
    Prepared = 2,     // These transactions have been written to all remote replicas and can be committed
    Committed = 4,    // If we crash, we can recover this page and rollforward
    Completed = 8,    // These can be cleared from the log as we are done here, they have been written and completed to disk
    TimedOut = 16,    // These can be cleared from the log as we are done here
    Completing = 32   // This should never occur on startup, this indicates the Background thread is completing the transaction and hardening the log page updates to the main store
}
