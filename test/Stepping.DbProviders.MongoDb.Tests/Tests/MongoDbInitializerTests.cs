using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shouldly;
using Stepping.Core.Databases;
using Xunit;

namespace Stepping.DbProviders.MongoDb.Tests.Tests;

[Collection(MongoTestCollection.Name)]
public class MongoDbInitializerTests : SteppingDbProvidersMongoTestBase
{
    protected MongoDbInitializer DbInitializer { get; }
    protected IBarrierCollectionProvider BarrierCollectionProvider { get; }

    public MongoDbInitializerTests()
    {
        DbInitializer = (MongoDbInitializer)ServiceProvider.GetRequiredService<IDbInitializer>();
        BarrierCollectionProvider = ServiceProvider.GetRequiredService<IBarrierCollectionProvider>();
    }

    [Fact]
    public async Task Should_Initialize_Database()
    {
        var client = new MongoClient(MongoDbFixture.ConnectionString);
        var database = client.GetDatabase(MongoDbTestConsts.Database);
        var steppingDbContext = new MongoDbSteppingDbContext(client, database, null, MongoDbFixture.ConnectionString);
        var barrierCollection = await BarrierCollectionProvider.GetAsync(steppingDbContext);

        await barrierCollection.Indexes.DropAllAsync();
        (await IsBarrierTableCreatedAsync(barrierCollection)).ShouldBeFalse();

        await DbInitializer.TryInitializeAsync(new MongoDbInitializingInfoModel(steppingDbContext));

        (await IsBarrierTableCreatedAsync(barrierCollection)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Throw_If_Duplicate_Initializing()
    {
        var client = new MongoClient(MongoDbFixture.ConnectionString);
        var database = client.GetDatabase(MongoDbTestConsts.Database);
        var steppingDbContext = new MongoDbSteppingDbContext(client, database, null, MongoDbFixture.ConnectionString);
        var barrierCollection = await BarrierCollectionProvider.GetAsync(steppingDbContext);

        await barrierCollection.Indexes.DropAllAsync();
        (await IsBarrierTableCreatedAsync(barrierCollection)).ShouldBeFalse();

        await DbInitializer.TryInitializeAsync(new MongoDbInitializingInfoModel(steppingDbContext));

        await DbInitializer.TryInitializeAsync(new MongoDbInitializingInfoModel(steppingDbContext));

        (await IsBarrierTableCreatedAsync(barrierCollection)).ShouldBeTrue();
    }

    protected static async Task<bool> IsBarrierTableCreatedAsync(IMongoCollection<SteppingBarrierDocument> collection)
    {
        return (await (await collection.Indexes.ListAsync()).ToListAsync()).Any(index =>
            index.GetElement("name").Value.ToString()!.Equals("gid_1_branch_id_1_op_1_barrier_id_1"));
    }
}