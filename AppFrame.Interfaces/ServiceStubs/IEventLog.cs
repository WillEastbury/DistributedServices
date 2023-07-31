namespace AppFrame.Interfaces;

public interface IEventLog
{
    // take an instance of a table
    ITable<EventLogRecord> _table {get;}
    // Provide methods to append to the table and to read from the table
    Task AppendRecordAsync(Severity severity, string LogStreamId, string message);
    Task<IEnumerable<EventLogRecord>> GetNextBatchRecordsAsync(string consumerId, string LogStreamId, int? LogRecordId, DateTime? start, DateTime? end, Severity? severityFilter);
    Task SetWaterMarkAsync(string consumerId, string LogStreamId, int LogRecordId);
    Task TruncateAsync(string LogStreamId, int LogRecordId);
}
public class EventLogRecord{
    public int LogRecordId {get; set;}
    public DateTime Timestamp {get; set;}
    public Severity Severity {get; set;}
    public string Message {get; set;}
}
public enum Severity{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}