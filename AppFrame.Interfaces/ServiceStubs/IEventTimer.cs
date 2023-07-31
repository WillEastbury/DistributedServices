namespace AppFrame.Interfaces;
public interface IEventTimer
{
    ITable<EventData> Table { get; }
    Task AddEvent(EventData eventData);
    Task CheckPointEvent(EventData eventData);
    Task RemoveEvent(string eventId);
    Task<IEnumerable<EventData>> GetEventsToTrigger();
}
public record EventData
{
    public string EventId { get; init; }
    public DateTime NextEventExecutingTime { get; set; }
    public string cronSpec { get; init; }
    public DateTime LastEventExecutedTime { get; set; }
    public string EventPayload { get; init; }
}