using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.TestBase.Fakes;

public class FakeDbTransactionContext : IDbTransactionContext
{
    public ISteppingDbContext DbContext { get; }

    public FakeDbTransactionContext(FakeSteppingDbContext dbContext)
    {
        if (!dbContext.IsTransactional)
        {
            throw new SteppingException("The specified DbContext is not with a DB transaction.");
        }

        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        ((FakeSteppingDbContext)DbContext).FakeCommitTransactionAsync();
}