using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Exceptions;

namespace Stepping.Core.Databases;

public class SteppingDbContextProviderResolver : ISteppingDbContextProviderResolver
{
    protected static Dictionary<string, Type>? CachedTypes { get; set; }

    private static readonly object SyncObj = new();

    protected IServiceProvider ServiceProvider { get; }

    public SteppingDbContextProviderResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual Task<ISteppingDbContextProvider> ResolveAsync(string dbProviderName)
    {
        if (CachedTypes is null)
        {
            lock (SyncObj)
            {
                if (CachedTypes is null)
                {
                    var providers = ServiceProvider.GetRequiredService<IEnumerable<ISteppingDbContextProvider>>();

                    var cacheTypes = providers.ToDictionary(provider => provider.DbProviderName,
                        provider => provider.GetType());

                    CachedTypes = cacheTypes;
                }
            }
        }

        if (!CachedTypes.ContainsKey(dbProviderName))
        {
            throw new SteppingException("Invalid DbProviderName.");
        }

        return Task.FromResult((ISteppingDbContextProvider)ServiceProvider.GetService(CachedTypes[dbProviderName]));
    }
}