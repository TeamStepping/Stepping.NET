using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Options;
using Stepping.TestBase;

namespace Stepping.TmProviders.LocalTm.Core.Tests;

public class SteppingTmProvidersLocalTmCoreTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSteppingLocalTm();

        base.ConfigureServices(services);
    }
}