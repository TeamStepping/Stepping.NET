using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.DbProviders.EfCore;

public class DefaultEfCoreSteppingDbContextProvider : ISteppingDbContextProvider
{
    public string DbProviderName => SteppingDbProviderEfCoreConsts.DbProviderName;

    protected IServiceProvider ServiceProvider { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected SteppingEfCoreOptions Options { get; }

    public DefaultEfCoreSteppingDbContextProvider(
        IServiceProvider serviceProvider,
        IConnectionStringHasher connectionStringHasher,
        IOptions<SteppingEfCoreOptions> options)
    {
        ServiceProvider = serviceProvider;
        ConnectionStringHasher = connectionStringHasher;
        Options = options.Value;
    }

    public virtual async Task<ISteppingDbContext> GetAsync(SteppingDbContextLookupInfoModel infoModel)
    {
        var dbContext = (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(
            ServiceProvider,
            Type.GetType(infoModel.DbContextType!)!
        );

        var connectionString = dbContext.Database.GetConnectionString() ?? Options.DefaultConnectionString;

        if (connectionString is null ||
            infoModel.HashedConnectionString != await ConnectionStringHasher.HashAsync(connectionString))
        {
            throw new SteppingException(
                "Invalid connection string. Please customize the connection string lookup implementation yourself.");
        }

        return new EfCoreSteppingDbContext(dbContext, infoModel.CustomInfo);
    }
}