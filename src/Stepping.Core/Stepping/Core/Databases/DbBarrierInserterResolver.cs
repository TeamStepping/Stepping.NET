using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stepping.Core.Exceptions;
using Stepping.Core.Options;

namespace Stepping.Core.Databases;

public class DbBarrierInserterResolver : IDbBarrierInserterResolver
{
    protected static Dictionary<string, Type>? CachedTypes { get; set; }

    private static readonly object SyncObj = new();

    protected IServiceProvider ServiceProvider { get; }

    public DbBarrierInserterResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual async Task<IDbBarrierInserter> ResolveAsync(ISteppingDbContext dbContext)
    {
        return await ResolveAsync(dbContext.DbProviderName);
    }

    public virtual Task<IDbBarrierInserter> ResolveAsync(string dbProviderName)
    {
        if (CachedTypes is null)
        {
            lock (SyncObj)
            {
                if (CachedTypes is null)
                {
                    var cacheTypes = new Dictionary<string, Type>();

                    using var scope = ServiceProvider.CreateScope();

                    var options = ServiceProvider.GetRequiredService<IOptions<SteppingOptions>>().Value;

                    foreach (var inserterType in options.DbBarrierInserters)
                    {
                        var inserter = (IDbBarrierInserter)ServiceProvider.GetRequiredService(inserterType);
                        cacheTypes.TryAdd(inserter.DbProviderName, inserterType);
                    }

                    CachedTypes = cacheTypes;
                }
            }
        }

        if (!CachedTypes.ContainsKey(dbProviderName))
        {
            throw new SteppingException("Invalid DbProviderName.");
        }

        return Task.FromResult((IDbBarrierInserter)ServiceProvider.GetRequiredService(CachedTypes[dbProviderName]));
    }
}