using System.Threading.Tasks;
namespace AppFrame.Interfaces;

public interface IPubSub
{
    ITable<Topic> tabletopics { get; } 
    ITable<TopicSubscription> tablesubscriptions { get; } 
    ITable<PubSubEntry> tablepubsubentries { get; } 

    Task<string> Enqueue(string topicName, byte[] message);
    Task Subscribe(string topicName, Uri callbackUri);
    Task Unsubscribe(string topicName, Uri callbackUri);
    Task AcknowledgeOK(string messageId, Uri callbackUri);
    Task AcknowledgeRetry(string messageId, Uri callbackUri);
    Task AcknowledgeFail(string messageId, Uri callbackUri);
    Task EventProcessorThread(); 
}
public class Topic
{
    public string TopicName {get;set;}
    public int MessageExpiryInDays {get;set;} = 30; 
    
}
public class TopicSubscription
{
    public string Id {get;set;} = Guid.NewGuid().ToString();
    public string SubscribeToTopicName {get;set;}
    public Uri CallBackUri {get;set;}
    public string AcknowledgeKey {get;set;}
}
public class PubSubEntry
{
    public string Key { get; set; } = Guid.NewGuid().ToString();
    public string TopicName { get; set; }
    public byte[] Message { get; set; }
    public DateTime SentAtUTC {get;set;} = DateTime.UtcNow;
    public List<string> DeliveredToIds {get;set;} = new List<string>();

}