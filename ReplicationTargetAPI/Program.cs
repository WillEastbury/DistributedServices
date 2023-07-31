using AppFrame.Implementations.Middleware;
using ReplicationTargetAPI;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ReplicationContextManager rcm = builder.BuildAndRegisterRCM();
WebApplication app = builder.Build();

app.UseMiddleware<APIKeyCheckMiddleware>();
app = rcm.RegisterReplicationContext(app);
app.Run();

