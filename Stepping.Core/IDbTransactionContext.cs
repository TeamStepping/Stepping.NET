namespace Stepping.Core;

public interface IDbTransactionContext
{
    ISteppingDbContext DbContext { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
}