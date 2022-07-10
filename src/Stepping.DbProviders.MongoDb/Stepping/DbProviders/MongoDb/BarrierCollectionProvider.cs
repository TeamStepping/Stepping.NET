using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Stepping.Core;
using Stepping.Core.Options;

namespace Stepping.DbProviders.MongoDb;

public class BarrierCollectionProvider : IBarrierCollectionProvider
{
    protected SteppingOptions Options { get; }

    public BarrierCollectionProvider(IOptions<SteppingOptions> options)
    {
        Options = options.Value;
    }
    
    public virtual Task<IMongoCollection<SteppingBarrierDocument>> GetAsync(MongoDbSteppingDbContext context)
    {
        var configuredTableName = Options.BarrierTableName ?? SteppingBarrierConsts.DefaultBarrierCollectionName;

        var fs = configuredTableName.Split('.');

        return Task.FromResult(fs.Length == 2
            ? context.Client.GetDatabase(fs[0]).GetCollection<SteppingBarrierDocument>(fs[1])
            : context.Database.GetCollection<SteppingBarrierDocument>(configuredTableName));
    }
}