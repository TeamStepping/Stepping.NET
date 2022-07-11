namespace Stepping.Core.Jobs;

public interface IDistributedJobStep
{
    Task DoAsync(IServiceProvider serviceProvider);
}