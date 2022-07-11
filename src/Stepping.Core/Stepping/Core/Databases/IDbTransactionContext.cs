namespace Stepping.Core.Databases;

public interface IDbTransactionContext
{
    ISteppingDbContext DbContext { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
}