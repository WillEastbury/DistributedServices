using System.Text;
using System.Text.Json;
using AppFrame.Implementations;
using AppFrame.Implementations.PageStorage;
using AppFrame.Implementations.TransactionLog;
using AppFrame.Interfaces;
using Microsoft.Extensions.Primitives;

namespace TableServiceAPI
{
    public class TableServiceContextManager<T> : ITableServiceContextManager<T>
    {
        ITable<T> TableOfT { get; set; }
        public TableServiceContextManager(ITable<T> Table)
        {
            TableOfT = Table;
        }
        public WebApplication RegisterTableServiceContextRoutes(WebApplication app)
        {
            string nameoft = nameof(T);
            app.MapPost("tables" + nameoft + "/{Key}", HandleCreate);
            app.MapPut("tables/" + nameoft + "/{Key}", HandleUpdate);
            app.MapGet("tables/" + nameoft + "/{Key}", HandleRead);
            app.MapGet("tables/query" + nameoft, HandleQuery);
            app.MapDelete("tables/" + nameoft + "/Key}", HandleDelete);
            app.MapPost("transactions", HandleCreateTransaction);
            app.MapDelete("transactions/{TransactionId}", HandleCancelTransaction);
            app.MapPut("transactions/{TransactionId}", HandleCommitTransaction);
            return app;
        }
        public async Task HandleCreateTransaction(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes((await TableOfT.BeginTransaction()).ToString()));
            context.Response.StatusCode = StatusCodes.Status200OK;
        }
        public async Task HandleCancelTransaction(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes((await TableOfT.CancelTransaction(context.Request.RouteValues["TransactionId"].ToString())).ToString()));
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        public async Task HandleCommitTransaction(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes((await TableOfT.CommitTransaction(context.Request.RouteValues["TransactionId"].ToString())).ToString()));
            context.Response.StatusCode = StatusCodes.Status200OK;
        }
        public async Task HandleCreate(HttpContext context)
        {
            string key = context.Request.RouteValues["Key"].ToString();
            T item = await DeserializeFromRequest(context.Request.Body);
            await TableOfT.Upsert(key, item, int.Parse(context.Request.Headers["TransactionId"]));
            context.Response.StatusCode = StatusCodes.Status201Created;
        }
        public async Task HandleDelete(HttpContext context)
        {
            string key = context.Request.RouteValues["Key"].ToString();
            await TableOfT.Delete(key, int.Parse(context.Request.Headers["TransactionId"]));
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        public async Task HandleQuery(HttpContext context)
        {
            var queryParameters = context.Request.Query;
            if (queryParameters.TryGetValue("where", out StringValues qp) && !string.IsNullOrWhiteSpace(qp.First()))
            {
                IEnumerable<T> results = await TableOfT.Query(qp.First());
                await WriteResponseList(context.Response, results);
            }
        }
        public async Task HandleRead(HttpContext context)
        {
            string key = context.Request.RouteValues["Key"].ToString();
            T item = await TableOfT.Read(key);
            if (item == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            else
            {
                await WriteResponse(context.Response, item);
            }
        }
        public async Task HandleUpdate(HttpContext context)
        {
            string key = context.Request.RouteValues["Key"].ToString();
            T item = await DeserializeFromRequest(context.Request.Body);
            await TableOfT.Upsert(key, item, int.Parse(context.Request.Headers["TransactionId"]));
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        private static async Task<T> DeserializeFromRequest(Stream body)
        {
            using var reader = new StreamReader(body, Encoding.UTF8);
            string content = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<T>(content);
        }

        private static async Task WriteResponse(HttpResponse response, T item)
        {
            response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(response.Body, item);
        }
        private static async Task WriteResponseList(HttpResponse response, IEnumerable<T> item)
        {
            response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(response.Body, item);
        }
    }
    public static class ServiceCollectionSetup
    {
        public static ITableServiceContextManager<T> RegisterTableServiceContextManagerAndDependencies<T>(IServiceCollection services, string DiskPath, TimeSpan CacheLifeTime)
        {
            Type entityType = typeof(T);
            ICache cache = new InMemoryCache();
            IPageStorageIO dbpsio = new DiskBackedPageStorageIO(DiskPath);
            ILocalPagedStorage lps = new LocalPagedStorage(dbpsio, cache, CacheLifeTime);
            List<IRemotePagedStorage> remotePagedStorages = new();   // ToDo: Populate with replicas from Cluster Data, for now leave it empty for a local only service with no remote replicas
            IPagedStorageReplicaSet pagedStorageReplicaSet = new PagedStorageReplicaSet(lps, remotePagedStorages);
            ITransactionLog transactionLog = new TransactionLog(pagedStorageReplicaSet);
            ITransactionalPagedStorageManager transactionalPagedStorageManager = new TransactionalPagedStorageManager(transactionLog, pagedStorageReplicaSet, 100);
            ILoggedKeyValueStore loggedKeyValueStore = new LoggedKeyValueStore(transactionalPagedStorageManager);
            Dictionary<string, ISearchTree> searchTrees = entityType.GetProperties().ToDictionary(propertyInfo => propertyInfo.Name, _ => (ISearchTree)new BinarySearchTree(transactionalPagedStorageManager, _.Name));
            IIndexedLoggedObjectStore indexedLoggedObjectStore = new IndexedLoggedObjectStore(loggedKeyValueStore, searchTrees);
            ITable<T> table = new Table<T>(indexedLoggedObjectStore);
            ITableServiceContextManager<T> tableServiceContextManager = new TableServiceContextManager<T>(table);

            services.AddSingleton(cache);
            services.AddSingleton(dbpsio);
            services.AddSingleton(lps);
            services.AddSingleton(remotePagedStorages);
            services.AddSingleton(pagedStorageReplicaSet);
            services.AddSingleton(transactionLog);
            services.AddSingleton(transactionalPagedStorageManager);
            services.AddSingleton(loggedKeyValueStore);
            services.AddSingleton(searchTrees);
            services.AddSingleton(indexedLoggedObjectStore);
            services.AddSingleton(table);
            services.AddSingleton(tableServiceContextManager);

            return tableServiceContextManager;

        }
    }
}