using Stepping.Core;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbTransactionContext : IDbTransactionContext
{
    public ISteppingDbContext DbContext { get; }

    public EfCoreDbTransactionContext(EfCoreSteppingDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await ((EfCoreSteppingDbContext)DbContext).DbContext.Database.CommitTransactionAsync(cancellationToken);
    }
}