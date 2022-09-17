using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.Core.Infrastructures;
using Xunit;

namespace Stepping.DbProviders.MongoDb.Tests.Tests;

[Collection(MongoTestCollection.Name)]
public class MongoSteppingDbContextProviderTests : SteppingDbProvidersMongoTestBase
{
    protected DefaultMongoDbSteppingDbContextProvider SteppingDbContextProvider { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected ISteppingTenantIdProvider TenantIdProvider { get; }

    public MongoSteppingDbContextProviderTests()
    {
        SteppingDbContextProvider = ServiceProvider.GetRequiredService<DefaultMongoDbSteppingDbContextProvider>();
        ConnectionStringHasher = ServiceProvider.GetRequiredService<IConnectionStringHasher>();
        TenantIdProvider = ServiceProvider.GetRequiredService<ISteppingTenantIdProvider>();
    }

    [Fact]
    public async Task Should_Get_SteppingDbContext()
    {
        SteppingDbContextProvider.DbProviderName.ShouldBe(SteppingDbProviderMongoDbConsts.DbProviderName);

        var dbContext = await SteppingDbContextProvider.GetAsync(new SteppingDbContextLookupInfoModel(
            SteppingDbProviderMongoDbConsts.DbProviderName,
            await ConnectionStringHasher.HashAsync(MongoDbFixture.ConnectionString),
            null,
            MongoDbTestConsts.Database,
            await TenantIdProvider.GetCurrentAsync(),
            "my-custom-info"));

        dbContext.ShouldNotBeNull();
        dbContext.ConnectionString.ShouldBe(MongoDbFixture.ConnectionString);
        dbContext.CustomInfo.ShouldBe("my-custom-info");
        dbContext.IsTransactional.ShouldBeFalse();
        dbContext.DbProviderName.ShouldBe(SteppingDbProviderMongoDbConsts.DbProviderName);
    }
}