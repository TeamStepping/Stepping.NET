using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.Databases;
using Stepping.Core.Steps;
using Stepping.Core.Tests.Fakes;
using Stepping.Core.TransactionManagers;
using Stepping.TestBase;

namespace Stepping.Core.Tests;

public abstract class SteppingCoreTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddStepping();

        services.AddTransient<IDbBarrierInserter, FakeDbBarrierInserter>();
        services.TryAddTransient<FakeDbBarrierInserter>();

        services.AddTransient<ISteppingDbContextProvider, FakeSteppingDbContextProvider>();
        services.TryAddTransient<FakeSteppingDbContextProvider>();

        services.AddTransient<ITmClient, FakeTmClient>();
        services.TryAddTransient<FakeTmClient>();

        services.AddTransient<IStep, FakeExecutableStep>();
        services.TryAddTransient<FakeExecutableStep>();

        services.AddTransient<IStep, FakeWithArgsExecutableStep>();
        services.TryAddTransient<FakeWithArgsExecutableStep>();

        services.TryAddTransient<FakeService>();
    }
}