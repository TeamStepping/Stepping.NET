using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Exceptions;

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
                    var inserters = ServiceProvider.GetRequiredService<IEnumerable<IDbBarrierInserter>>();

                    var cacheTypes = inserters.ToDictionary(inserter => inserter.DbProviderName,
                        inserter => inserter.GetType());

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