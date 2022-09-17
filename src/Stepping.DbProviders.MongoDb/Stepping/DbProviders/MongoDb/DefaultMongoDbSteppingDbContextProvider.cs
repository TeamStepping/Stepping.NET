using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;

namespace Stepping.DbProviders.MongoDb;

public class DefaultMongoDbSteppingDbContextProvider : ISteppingDbContextProvider
{
    public string DbProviderName => SteppingDbProviderMongoDbConsts.DbProviderName;

    protected IServiceProvider ServiceProvider { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected SteppingMongoDbOptions Options { get; }

    public DefaultMongoDbSteppingDbContextProvider(
        IServiceProvider serviceProvider,
        IConnectionStringHasher connectionStringHasher,
        IOptions<SteppingMongoDbOptions> options)
    {
        ServiceProvider = serviceProvider;
        ConnectionStringHasher = connectionStringHasher;
        Options = options.Value;
    }

    public virtual async Task<ISteppingDbContext> GetAsync(SteppingDbContextLookupInfoModel infoModel)
    {
        var connectionString = Options.DefaultConnectionString;

        if (infoModel.HashedConnectionString != await ConnectionStringHasher.HashAsync(connectionString))
        {
            throw new SteppingException(
                "Invalid connection string. Please customize the connection string lookup implementation yourself.");
        }

        var client = new MongoClient(connectionString);

        return new MongoDbSteppingDbContext(
            client,
            client.GetDatabase(infoModel.Database),
            null,
            connectionString,
            infoModel.CustomInfo);
    }
}