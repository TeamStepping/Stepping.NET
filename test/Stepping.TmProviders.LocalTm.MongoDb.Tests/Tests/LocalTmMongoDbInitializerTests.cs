using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shouldly;
using Xunit;

namespace Stepping.TmProviders.LocalTm.MongoDb.Tests.Tests;

[Collection(MongoTestCollection.Name)]
public class LocalTmMongoDbInitializerTests : SteppingTmProvidersLocalTmMongoDbTestBase
{
    protected ILocalTmMongoDbInitializer DbInitializer { get; }
    protected LocalTmMongoDbContext LocalTmMongoDbContext { get; }

    public LocalTmMongoDbInitializerTests()
    {
        DbInitializer = ServiceProvider.GetRequiredService<LocalTmMongoDbInitializer>();
        LocalTmMongoDbContext = ServiceProvider.GetRequiredService<LocalTmMongoDbContext>();
    }

    protected override void ConfigureServices(ServiceCollection services)
    {
        base.ConfigureServices(services);

        services.Configure<LocalTmMongoDbOptions>(options =>
        {
            options.DatabaseName = $"db_{Guid.NewGuid():N}";
        });
    }

    [Fact]
    public async Task Should_Initialize_Database()
    {
        var tmTransactionCollection = LocalTmMongoDbContext.GetTmTransactionCollection();
        await tmTransactionCollection.Indexes.DropAllAsync();

        (await IsTableCreatedAsync(tmTransactionCollection)).ShouldBeFalse();

        await DbInitializer.TryInitializeAsync();

        (await IsTableCreatedAsync(tmTransactionCollection)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Throw_If_Duplicate_Initializing()
    {
        var tmTransactionCollection = LocalTmMongoDbContext.GetTmTransactionCollection();
        await tmTransactionCollection.Indexes.DropAllAsync();

        (await IsTableCreatedAsync(tmTransactionCollection)).ShouldBeFalse();

        await DbInitializer.TryInitializeAsync();

        await DbInitializer.TryInitializeAsync();

        (await IsTableCreatedAsync(tmTransactionCollection)).ShouldBeTrue();
    }

    protected static async Task<bool> IsTableCreatedAsync(IMongoCollection<TmTransactionDocument> collection)
    {
        return (await (await collection.Indexes.ListAsync()).ToListAsync()).Any(index =>
            index.GetElement("name").Value.ToString()!.Equals("Gid_1"));
    }
}
