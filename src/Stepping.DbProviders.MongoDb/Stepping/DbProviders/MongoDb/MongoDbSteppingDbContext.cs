using MongoDB.Driver;
using Stepping.Core;
using Stepping.Core.Databases;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbSteppingDbContext : ISteppingDbContext
{
    public IMongoDatabase Database { get; }
    
    public IMongoClient Client { get; }

    public IClientSessionHandle SessionHandle { get; }

    public string ConnectionString { get; }

    public MongoDbSteppingDbContext(
        IMongoDatabase database, IMongoClient client, IClientSessionHandle sessionHandle, string connectionString)
    {
        Database = database;
        Client = client;
        SessionHandle = sessionHandle;
        ConnectionString = connectionString;
    }
}