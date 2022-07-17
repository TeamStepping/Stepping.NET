namespace Stepping.Core.Databases;

public interface ISteppingDbContext
{
    string DbProviderName { get; }

    string ConnectionString { get; }
    
    bool IsTransactional { get; }
    
    public string? CustomInfo { get; }

    Type? GetInternalDbContextTypeOrNull();

    string? GetInternalDatabaseNameOrNull();
    
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}