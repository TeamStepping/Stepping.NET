namespace Stepping.Core.Jobs;

public interface IDistributedJobGidGenerator
{
    Task<string> CreateAsync();
}