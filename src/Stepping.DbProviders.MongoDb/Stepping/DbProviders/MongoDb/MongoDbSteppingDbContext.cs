using MongoDB.Driver;
using Stepping.Core.Databases;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbSteppingDbContext : SteppingDbContextBase
{
    public override string DbProviderName => SteppingDbProviderMongoDbConsts.DbProviderName;

    public IMongoDatabase Database { get; }

    public IMongoClient Client { get; }

    public IClientSessionHandle? SessionHandle { get; }

    public override string ConnectionString { get; }

    public override bool IsTransactional => SessionHandle is not null && SessionHandle.IsInTransaction;

    public override Type? GetInternalDbContextTypeOrNull() => null;

    public override string? GetInternalDatabaseNameOrNull() => Database.DatabaseNamespace.DatabaseName;

    public MongoDbSteppingDbContext(
        IMongoDatabase database,
        IMongoClient client,
        IClientSessionHandle? sessionHandle,
        string connectionString,
        string? customInfo = null) : base(customInfo)
    {
        Database = database;
        Client = client;
        SessionHandle = sessionHandle;
        ConnectionString = connectionString;
    }

    protected override async Task InternalCommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await SessionHandle!.CommitTransactionAsync(cancellationToken);
}