using Stepping.Core.Databases;

namespace Stepping.TestBase.Fakes;

public class FakeSteppingDbContext : SteppingDbContextBase
{
    public const string FakeDbProviderName = "Fake";
    public const string FakeConnectionString = "my-connection-string";

    public override string DbProviderName => FakeDbProviderName;

    public override string ConnectionString => FakeConnectionString;

    public override bool IsTransactional { get; }

    public override Type? GetInternalDbContextTypeOrNull() => null;

    public override string? GetInternalDatabaseNameOrNull() => null;

    protected override Task InternalCommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (TransactionCommitted)
        {
            throw new Exception();
        }

        TransactionCommitted = true;

        return Task.CompletedTask;
    }

    public bool TransactionCommitted { get; private set; }

    public FakeSteppingDbContext(bool isTransactional)
    {
        IsTransactional = isTransactional;
    }
}