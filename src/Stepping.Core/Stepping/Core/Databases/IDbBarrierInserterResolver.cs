namespace Stepping.Core.Databases;

public interface IDbBarrierInserterResolver
{
    Task<IDbBarrierInserter> ResolveAsync(ISteppingDbContext dbContext);

    Task<IDbBarrierInserter> ResolveAsync(string dbProviderName);
}