using AppFrame.Interfaces;
public class ReplicationContextManager : IReplicationContextManager
{
    private ILocalPagedStorage storage {get;set;}
    public ReplicationContextManager(ILocalPagedStorage localPagedStorage)
    {
        storage = localPagedStorage;
    }
    public WebApplication RegisterReplicationContext(WebApplication app)
    {
        app.MapGet("repl/page/{fileId}/{pageNumber}", HandleGetPage);
        app.MapPut("repl/page/{fileId}/{pageNumber}", HandleSetPage);
        app.MapDelete("repl/page/{fileId}/{pageNumber}", HandleDeletePage);
        return app;
    }
    public async Task HandleGetPage(HttpContext context)
    {
        string fileId = context.Request.RouteValues["fileId"]?.ToString();
        int pageNumber = int.Parse(context.Request.RouteValues["pageNumber"]?.ToString() ?? "0");
        byte[] data = await storage.GetPageAsync(fileId, pageNumber);
        if (data != null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.Body.WriteAsync(data);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
    public async Task HandleSetPage(HttpContext context)
    {
        
        string fileId = context.Request.RouteValues["fileId"]?.ToString();
        int pageNumber = int.Parse(context.Request.RouteValues["pageNumber"]?.ToString() ?? "0");
        using (var memoryStream = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(memoryStream);
            byte[] data = memoryStream.ToArray();
            await storage.SetPageAsync(fileId, pageNumber, data);
        }
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }
    public async Task HandleDeletePage(HttpContext context)
    {
        string fileId = context.Request.RouteValues["fileId"]?.ToString();
        int pageNumber = int.Parse(context.Request.RouteValues["pageNumber"]?.ToString() ?? "0");
        await storage.DeletePageAsync(fileId, pageNumber);
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }
}