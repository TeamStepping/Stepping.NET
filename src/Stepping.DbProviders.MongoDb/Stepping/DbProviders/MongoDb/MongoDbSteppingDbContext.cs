using MongoDB.Driver;
using Stepping.Core.Databases;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbSteppingDbContext : SteppingDbContextBase
{
    public override string DbProviderName => SteppingDbProviderMongoDbConsts.DbProviderName;

    /// <summary>
    /// Specify the database for the Barrier collection.
    /// </summary>
    public IMongoDatabase BarrierDatabase { get; }

    public IMongoClient Client { get; }

    public IClientSessionHandle? SessionHandle { get; }

    public override string ConnectionString { get; }

    public override bool IsTransactional => SessionHandle is not null && SessionHandle.IsInTransaction;

    public override Type? GetInternalDbContextTypeOrNull() => null;

    public override string? GetInternalDatabaseNameOrNull() => BarrierDatabase.DatabaseNamespace.DatabaseName;

    public MongoDbSteppingDbContext(
        IMongoClient client,
        IMongoDatabase barrierDatabase,
        IClientSessionHandle? sessionHandle,
        string connectionString,
        string? customInfo = null) : base(customInfo)
    {
        Client = client;
        BarrierDatabase = barrierDatabase;
        SessionHandle = sessionHandle;
        ConnectionString = connectionString;
    }

    protected override async Task InternalCommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await SessionHandle!.CommitTransactionAsync(cancellationToken);
}