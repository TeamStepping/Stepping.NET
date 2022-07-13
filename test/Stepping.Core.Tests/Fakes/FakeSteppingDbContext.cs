using Stepping.Core.Databases;

namespace Stepping.Core.Tests.Fakes;

public class FakeSteppingDbContext : ISteppingDbContext
{
    public const string FakeDbProviderName = "Fake";

    public string DbProviderName => FakeDbProviderName;

    public string ConnectionString => "my-connection-string";

    public Type? GetInternalDbContextTypeOrNull() => null;

    public string? GetInternalDatabaseNameOrNull() => null;

    public bool IsTransactional { get; }

    public bool TransactionCommitted { get; private set; }

    public FakeSteppingDbContext(bool isTransactional)
    {
        IsTransactional = isTransactional;
    }

    public virtual Task FakeCommitTransactionAsync()
    {
        if (!IsTransactional || TransactionCommitted)
        {
            throw new Exception();
        }

        TransactionCommitted = true;

        return Task.CompletedTask;
    }
}