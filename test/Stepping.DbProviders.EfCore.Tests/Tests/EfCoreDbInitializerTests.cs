using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.DbProviders.EfCore.Tests.Fakes;
using Xunit;

namespace Stepping.DbProviders.EfCore.Tests.Tests;

public class EfCoreDbInitializerTests : SteppingDbProvidersEfCoreTestBase
{
    protected EfCoreDbInitializer DbInitializer { get; }

    public EfCoreDbInitializerTests()
    {
        DbInitializer = (EfCoreDbInitializer)ServiceProvider.GetRequiredService<IDbInitializer>();
    }

    [Fact]
    public async Task Should_Initialize_Database()
    {
        var dbContext = ServiceProvider.GetRequiredService<FakeDbContext>();

        (await IsBarrierTableCreatedAsync(dbContext)).ShouldBeFalse();

        await DbInitializer.TryInitializeAsync(
            new EfCoreDbInitializingInfoModel(new EfCoreSteppingDbContext(dbContext)));

        (await IsBarrierTableCreatedAsync(dbContext)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Throw_If_Duplicate_Initializing()
    {
        var dbContext = ServiceProvider.GetRequiredService<FakeDbContext>();

        (await IsBarrierTableCreatedAsync(dbContext)).ShouldBeFalse();

        await DbInitializer.TryInitializeAsync(
            new EfCoreDbInitializingInfoModel(new EfCoreSteppingDbContext(dbContext)));

        await Should.NotThrowAsync(() => DbInitializer.TryInitializeAsync(
            new EfCoreDbInitializingInfoModel(new EfCoreSteppingDbContext(dbContext))));

        (await IsBarrierTableCreatedAsync(dbContext)).ShouldBeTrue();
    }

    protected static async Task<bool> IsBarrierTableCreatedAsync(DbContext dbContext)
    {
        var result = await dbContext.Database
            .GetDbConnection()
            .QueryFirstAsync<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='stepping_barrier';");

        return result == 1;
    }
}