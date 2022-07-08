namespace Stepping.Core;

public interface IDistributedJobStep
{
    Task DoAsync(IServiceProvider serviceProvider);
}