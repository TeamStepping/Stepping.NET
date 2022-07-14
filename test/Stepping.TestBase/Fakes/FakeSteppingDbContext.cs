using Stepping.Core.Databases;

namespace Stepping.TestBase.Fakes;

public class FakeSteppingDbContext : ISteppingDbContext
{
    public const string FakeDbProviderName = "Fake";
    public const string FakeConnectionString = "my-connection-string";

    public string DbProviderName => FakeDbProviderName;

    public string ConnectionString => FakeConnectionString;

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