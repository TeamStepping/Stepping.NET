namespace Stepping.Core.Jobs;

public interface IAtomicJobGidGenerator
{
    Task<string> CreateAsync();
}