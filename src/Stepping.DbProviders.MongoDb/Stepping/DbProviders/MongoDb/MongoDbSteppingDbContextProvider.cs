using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Stepping.Core.Databases;
using Stepping.Core.Secrets;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbSteppingDbContextProvider : ISteppingDbContextProvider
{
    public string DbProviderName => SteppingDbProviderMongoDbConsts.DbProviderName;

    protected IServiceProvider ServiceProvider { get; }
    protected IConnectionStringEncryptor ConnectionStringEncryptor { get; }

    public MongoDbSteppingDbContextProvider(
        IServiceProvider serviceProvider,
        IConnectionStringEncryptor connectionStringEncryptor)
    {
        ServiceProvider = serviceProvider;
        ConnectionStringEncryptor = connectionStringEncryptor;
    }

    public virtual async Task<ISteppingDbContext> GetAsync(SteppingDbContextInfoModel infoModel)
    {
        var connectionString = await ConnectionStringEncryptor.DecryptAsync(infoModel.EncryptedConnectionString);
        var client = new MongoClient(connectionString);

        return new MongoDbSteppingDbContext(client.GetDatabase(infoModel.Database), client, null, connectionString);
    }
}