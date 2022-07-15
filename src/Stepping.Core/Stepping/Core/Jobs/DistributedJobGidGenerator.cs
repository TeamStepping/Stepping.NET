namespace Stepping.Core.Jobs;

public class DistributedJobGidGenerator : IDistributedJobGidGenerator
{
    public Task<string> CreateAsync() => Task.FromResult(Guid.NewGuid().ToString());
}