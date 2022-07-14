using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class DbBarrierInserterResolverTests : SteppingCoreTestBase
{
    protected IDbBarrierInserterResolver DbBarrierInserterResolver { get; }

    public DbBarrierInserterResolverTests()
    {
        DbBarrierInserterResolver = ServiceProvider.GetRequiredService<IDbBarrierInserterResolver>();
    }

    [Fact]
    public async Task Should_Resolve_By_DbContext()
    {
        var inserter = await DbBarrierInserterResolver.ResolveAsync(new FakeSteppingDbContext(false));

        inserter.ShouldNotBeNull();
        inserter.DbProviderName.ShouldBe(FakeSteppingDbContext.FakeDbProviderName);
    }

    [Fact]
    public async Task Should_Resolve_By_DbProviderName()
    {
        var inserter = await DbBarrierInserterResolver.ResolveAsync(FakeSteppingDbContext.FakeDbProviderName);

        inserter.ShouldNotBeNull();
        inserter.DbProviderName.ShouldBe(FakeSteppingDbContext.FakeDbProviderName);
    }
}