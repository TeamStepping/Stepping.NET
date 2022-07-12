using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbTransactionContext : IDbTransactionContext
{
    public ISteppingDbContext DbContext { get; }

    public MongoDbTransactionContext(MongoDbSteppingDbContext dbContext)
    {
        if (dbContext.SessionHandle is null)
        {
            throw new SteppingException("The specified DbContext is not with a DB transaction.");
        }

        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var sessionHandle = ((MongoDbSteppingDbContext)DbContext).SessionHandle!;

        await sessionHandle.CommitTransactionAsync(cancellationToken);
    }
}