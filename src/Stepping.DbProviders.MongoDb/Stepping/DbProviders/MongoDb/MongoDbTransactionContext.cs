using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbTransactionContext : IDbTransactionContext
{
    public ISteppingDbContext DbContext { get; }

    public MongoDbTransactionContext(MongoDbSteppingDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var sessionHandle = ((MongoDbSteppingDbContext)DbContext).SessionHandle;

        if (sessionHandle is null)
        {
            throw new SteppingException("This job is not with a DB transaction.");
        }

        await sessionHandle.CommitTransactionAsync(cancellationToken);
    }
}