using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.Core.Tests.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class SteppingDbContextProviderResolverTests : SteppingCoreTestBase
{
    protected ISteppingDbContextProviderResolver SteppingDbContextProviderResolver { get; }

    public SteppingDbContextProviderResolverTests()
    {
        SteppingDbContextProviderResolver = ServiceProvider.GetRequiredService<ISteppingDbContextProviderResolver>();
    }

    [Fact]
    public async Task Should_Resolve_By_DbProviderName()
    {
        var dbContextProvider =
            await SteppingDbContextProviderResolver.ResolveAsync(FakeSteppingDbContext.FakeDbProviderName);

        dbContextProvider.ShouldNotBeNull();
        dbContextProvider.DbProviderName.ShouldBe(FakeSteppingDbContext.FakeDbProviderName);
    }
}