using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Stepping.Core;
using Stepping.Core.Databases;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbInitializer : IDbInitializer
{
    protected IBarrierCollectionProvider BarrierCollectionProvider { get; }
    private ILogger<MongoDbInitializer> Logger { get; }
    private ConcurrentDictionary<string, bool> CreatedServers { get; } = new();


    public MongoDbInitializer(
        IBarrierCollectionProvider barrierCollectionProvider,
        ILogger<MongoDbInitializer> logger)
    {
        BarrierCollectionProvider = barrierCollectionProvider;
        Logger = logger;
    }

    public virtual async Task TryInitializeAsync(IDbInitializingInfoModel infoModel)
    {
        var dbContext = (MongoDbSteppingDbContext)infoModel.DbContext;

        var servers = dbContext.Database.Client.Settings.Servers.Select(x => x.ToString()).ToList();

        if (servers.All(x => CreatedServers.ContainsKey(x)))
        {
            return;
        }

        var mongoCollection = await BarrierCollectionProvider.GetAsync(dbContext);

        await mongoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SteppingBarrierDocument>(
                "{ gid: 1, branch_id: 1, op: 1, barrier_id: 1 }", new CreateIndexOptions
                {
                    Unique = true
                }));

        foreach (var server in servers)
        {
            CreatedServers[server] = true;
        }
    }
}