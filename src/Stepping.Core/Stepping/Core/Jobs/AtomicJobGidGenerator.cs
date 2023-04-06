namespace Stepping.Core.Jobs;

public class AtomicJobGidGenerator : IAtomicJobGidGenerator
{
    public Task<string> CreateAsync() => Task.FromResult(Guid.NewGuid().ToString());
}