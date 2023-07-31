using AppFrame.Implementations.Middleware;
using TableServiceAPI;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ITableServiceContextManager<Person> rcm = ServiceCollectionSetup.RegisterTableServiceContextManagerAndDependencies<Person>(builder.Services, "C:\\ReplLocal1\\", TimeSpan.FromSeconds(180));
WebApplication app = builder.Build();

app.UseMiddleware<APIKeyCheckMiddleware>();
app = rcm.RegisterTableServiceContextRoutes(app);
app.Run();
