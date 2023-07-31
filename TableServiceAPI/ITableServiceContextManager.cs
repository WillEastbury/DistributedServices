public interface ITableServiceContextManager<T>
{
    WebApplication RegisterTableServiceContextRoutes(WebApplication app);
    Task HandleCreate(HttpContext context);
    Task HandleUpdate(HttpContext context);
    Task HandleRead(HttpContext context);
    Task HandleQuery(HttpContext context);
    Task HandleDelete(HttpContext context);
}