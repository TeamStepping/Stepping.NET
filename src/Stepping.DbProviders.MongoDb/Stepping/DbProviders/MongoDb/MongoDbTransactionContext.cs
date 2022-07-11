using Stepping.Core;
using Stepping.Core.Databases;

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
        await ((MongoDbSteppingDbContext)DbContext).SessionHandle!.CommitTransactionAsync(cancellationToken);
    }
}