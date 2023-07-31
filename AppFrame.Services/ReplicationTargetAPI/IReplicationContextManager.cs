public interface IReplicationContextManager
{
    WebApplication RegisterReplicationContext(WebApplication app);
    Task HandleGetPage(HttpContext context);
    Task HandleSetPage(HttpContext context);
    Task HandleDeletePage(HttpContext context);
    
}
