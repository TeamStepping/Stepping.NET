using System.Collections.Concurrent;
using System.Threading;
using MongoDB.Driver;

namespace Stepping.TmProviders.LocalTm.MongoDb;

internal class LocalTmMongoDbInitializer : ILocalTmMongoDbInitializer
{
    protected LocalTmMongoDbContext LocalTmMongoDbContext { get; }

    protected static ConcurrentDictionary<string, bool> CreatedServers { get; } = new();
    public static bool CacheEnabled { get; set; } = true;
    private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public LocalTmMongoDbInitializer(LocalTmMongoDbContext localTmMongoDbContext)
    {
        LocalTmMongoDbContext = localTmMongoDbContext;
    }

    public virtual async Task TryInitializeAsync()
    {
        var client = LocalTmMongoDbContext.GetMongoClient();

        var servers = client.Settings.Servers.Select(x => x.ToString()).ToList();

        if (CacheEnabled && servers.All(x => CreatedServers.ContainsKey(x)))
        {
            return;
        }
        try
        {
            await _semaphoreSlim.WaitAsync();

            var mongoCollection = LocalTmMongoDbContext.GetTmTransactionCollection();

            await mongoCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<TmTransactionDocument>(
                    Builders<TmTransactionDocument>.IndexKeys
                        .Ascending(x => x.Gid),
                    new CreateIndexOptions
                    {
                        Unique = true
                    })
            );

            await mongoCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<TmTransactionDocument>(
                    Builders<TmTransactionDocument>.IndexKeys
                        .Ascending(x => x.Status)
                        .Ascending(x => x.NextRetryTime))
            );

            foreach (var server in servers)
            {
                CreatedServers[server] = true;
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
