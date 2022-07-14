using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.DbProviders.EfCore.Tests.Fakes;
using Xunit;

namespace Stepping.DbProviders.EfCore.Tests.Tests;

public class EfCoreSteppingDbContextProviderTests : SteppingDbProvidersEfCoreTestBase
{
    protected EfCoreSteppingDbContextProvider SteppingDbContextProvider { get; }

    public EfCoreSteppingDbContextProviderTests()
    {
        SteppingDbContextProvider = ServiceProvider.GetRequiredService<EfCoreSteppingDbContextProvider>();
    }

    [Fact]
    public async Task Should_Get_SteppingDbContext()
    {
        SteppingDbContextProvider.DbProviderName.ShouldBe(SteppingDbProviderEfCoreConsts.DbProviderName);

        var dbContext = await SteppingDbContextProvider.GetAsync(new SteppingDbContextInfoModel(
            SteppingDbProviderEfCoreConsts.DbProviderName,
            SteppingDbContextInfoModel.GetTypeFullNameWithAssemblyName(typeof(FakeDbContext)),
            null,
            FakeDbContext.ConnectionString));

        dbContext.ShouldNotBeNull();
    }
}