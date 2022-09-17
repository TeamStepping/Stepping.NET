using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Infrastructures;
using Stepping.Core.Tests.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class MultiTenancyTests : SteppingCoreTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddTransient<ISteppingTenantIdProvider, FakeSteppingTenantIdProvider>();
        services.AddTransient<FakeSteppingTenantIdProvider>();

        base.ConfigureServices(services);
    }

    [Fact]
    public async Task Should_Get_Tenant_Id()
    {
        var provider = ServiceProvider.GetRequiredService<ISteppingTenantIdProvider>();

        (await provider.GetCurrentAsync()).ShouldBe(FakeSteppingTenantIdProvider.MyTenantId);
    }
}