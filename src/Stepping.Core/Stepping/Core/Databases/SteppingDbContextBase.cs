using Stepping.Core.Exceptions;

namespace Stepping.Core.Databases;

public abstract class SteppingDbContextBase : ISteppingDbContext
{
    public abstract string DbProviderName { get; }

    public abstract string ConnectionString { get; }

    public abstract bool IsTransactional { get; }

    public abstract Type? GetInternalDbContextTypeOrNull();

    public abstract string? GetInternalDatabaseNameOrNull();

    protected abstract Task InternalCommitTransactionAsync(CancellationToken cancellationToken = default);

    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        CheckTransactionalExists();

        await InternalCommitTransactionAsync(cancellationToken);
    }

    protected virtual void CheckTransactionalExists()
    {
        if (!IsTransactional)
        {
            throw new SteppingException("The DB context is not with a transaction.");
        }
    }
}