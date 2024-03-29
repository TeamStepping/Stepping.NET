﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.Databases;
using Stepping.Core.TransactionManagers;
using Stepping.TestBase.Fakes;

namespace Stepping.TestBase;

public abstract class SteppingTestBase
{
    protected IServiceProvider ServiceProvider { get; }

    public SteppingTestBase()
    {
        var serviceCollection = new ServiceCollection();

        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        AfterBuildingServiceProvider();
    }

    protected virtual void AfterBuildingServiceProvider()
    {
    }

    protected virtual void ConfigureServices(ServiceCollection services)
    {
        services.AddStepping(options =>
        {
            options.RegisterSteps(typeof(SteppingTestBase).Assembly);
            options.RegisterDbBarrierInserters(typeof(FakeDbBarrierInserter));
        });

        services.AddLogging();

        services.AddSingleton<FakeDbBarrierInserter>();
        services.TryAddTransient<IDbBarrierInserter>(sp => sp.GetRequiredService<FakeDbBarrierInserter>());

        services.AddTransient<ISteppingDbContextProvider, FakeSteppingDbContextProvider>();
        services.TryAddTransient<FakeSteppingDbContextProvider>();

        services.AddTransient<ITmClient, FakeTmClient>();
        services.TryAddTransient<FakeTmClient>();

        services.TryAddTransient<FakeService>();
    }
}