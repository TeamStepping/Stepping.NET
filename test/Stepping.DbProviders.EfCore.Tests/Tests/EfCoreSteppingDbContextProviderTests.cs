using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.Core.Extensions;
using Stepping.Core.Infrastructures;
using Stepping.DbProviders.EfCore.Tests.Fakes;
using Xunit;

namespace Stepping.DbProviders.EfCore.Tests.Tests;

public class EfCoreSteppingDbContextProviderTests : SteppingDbProvidersEfCoreTestBase
{
    protected DefaultEfCoreSteppingDbContextProvider SteppingDbContextProvider { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected ISteppingTenantIdProvider TenantIdProvider { get; }

    public EfCoreSteppingDbContextProviderTests()
    {
        SteppingDbContextProvider = ServiceProvider.GetRequiredService<DefaultEfCoreSteppingDbContextProvider>();
        ConnectionStringHasher = ServiceProvider.GetRequiredService<IConnectionStringHasher>();
        TenantIdProvider = ServiceProvider.GetRequiredService<ISteppingTenantIdProvider>();
    }

    [Fact]
    public async Task Should_Get_SteppingDbContext()
    {
        SteppingDbContextProvider.DbProviderName.ShouldBe(SteppingDbProviderEfCoreConsts.DbProviderName);

        var dbContext = await SteppingDbContextProvider.GetAsync(new SteppingDbContextLookupInfoModel(
            SteppingDbProviderEfCoreConsts.DbProviderName,
            await ConnectionStringHasher.HashAsync(FakeDbContext.ConnectionString),
            typeof(FakeDbContext).GetTypeFullNameWithAssemblyName(),
            null,
            await TenantIdProvider.GetCurrentAsync(),
            "my-custom-info"));

        dbContext.ShouldNotBeNull();
        dbContext.ConnectionString.ShouldBe(FakeDbContext.ConnectionString);
        dbContext.CustomInfo.ShouldBe("my-custom-info");
        dbContext.IsTransactional.ShouldBeFalse();
        dbContext.DbProviderName.ShouldBe(SteppingDbProviderEfCoreConsts.DbProviderName);
    }
}