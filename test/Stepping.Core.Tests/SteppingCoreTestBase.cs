﻿using Microsoft.Extensions.DependencyInjection;
using Stepping.TestBase;

namespace Stepping.Core.Tests;

public abstract class SteppingCoreTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddStepping();

        base.ConfigureServices(services);
    }
}