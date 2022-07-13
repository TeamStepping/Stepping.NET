using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbTransactionContext : IDbTransactionContext
{
    public ISteppingDbContext DbContext { get; }

    public EfCoreDbTransactionContext(EfCoreSteppingDbContext dbContext)
    {
        if (dbContext.DbContext.Database.CurrentTransaction is null)
        {
            throw new SteppingException("The specified DbContext is not with a DB transaction.");
        }

        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var efDbContext = ((EfCoreSteppingDbContext)DbContext).DbContext;

        await efDbContext.Database.CommitTransactionAsync(cancellationToken);
    }
}