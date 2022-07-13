using Microsoft.Extensions.DependencyInjection;

namespace Stepping.TestBase;

public abstract class SteppingTestBase
{
    protected IServiceProvider ServiceProvider { get; }
    
    public SteppingTestBase()
    {
        var serviceCollection = new ServiceCollection();

        ConfigureServices(serviceCollection);
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(ServiceCollection services)
    {
    }
}