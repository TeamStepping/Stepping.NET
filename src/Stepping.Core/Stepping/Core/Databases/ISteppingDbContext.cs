namespace Stepping.Core.Databases;

public interface ISteppingDbContext
{
    string DbProviderName { get; }

    string ConnectionString { get; }
    
    bool IsTransactional { get; }

    Type? GetInternalDbContextTypeOrNull();

    string? GetInternalDatabaseNameOrNull();
    
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}