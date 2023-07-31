using AppFrame.Implementations;
using AppFrame.Implementations.PageStorage;
using AppFrame.Interfaces;

namespace ReplicationTargetAPI;

public static class RCMExtensions 
{
    public static ReplicationContextManager BuildAndRegisterRCM(this WebApplicationBuilder builder)
    {
        var cache = new InMemoryCache();
        var dbpsio = new DiskBackedPageStorageIO("C:\\ReplLocal1\\");
        var lps = new LocalPagedStorage(dbpsio, cache, TimeSpan.FromSeconds(180));
        var rcm = new ReplicationContextManager(lps);

        builder.Services.AddSingleton<ICache, InMemoryCache>(e => cache);
        builder.Services.AddSingleton<IPageStorageIO, DiskBackedPageStorageIO>(e => dbpsio);
        builder.Services.AddSingleton<ILocalPagedStorage, LocalPagedStorage>(e => lps);
        builder.Services.AddSingleton<IReplicationContextManager, ReplicationContextManager>();
        return rcm;
    }
}
