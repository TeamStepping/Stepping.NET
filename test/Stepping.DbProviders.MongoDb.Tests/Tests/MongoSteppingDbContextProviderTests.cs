using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Xunit;

namespace Stepping.DbProviders.MongoDb.Tests.Tests;

[Collection(MongoTestCollection.Name)]
public class MongoSteppingDbContextProviderTests : SteppingDbProvidersMongoTestBase
{
    protected MongoDbSteppingDbContextProvider SteppingDbContextProvider { get; }

    public MongoSteppingDbContextProviderTests()
    {
        SteppingDbContextProvider = ServiceProvider.GetRequiredService<MongoDbSteppingDbContextProvider>();
    }

    [Fact]
    public async Task Should_Get_SteppingDbContext()
    {
        SteppingDbContextProvider.DbProviderName.ShouldBe(SteppingDbProviderMongoDbConsts.DbProviderName);
    
        var dbContext = await SteppingDbContextProvider.GetAsync(new SteppingDbContextInfoModel(
            SteppingDbProviderMongoDbConsts.DbProviderName,
            null,
            MongoDbTestConsts.Database,
            MongoDbFixture.ConnectionString));
    
        dbContext.ShouldNotBeNull();
    }
}