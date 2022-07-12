using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

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
        var efDbContext = ((EfCoreSteppingDbContext)DbContext).DbContext;

        if (efDbContext.Database.CurrentTransaction is null)
        {
            throw new SteppingException("This job is not with a DB transaction.");
        }

        await efDbContext.Database.CommitTransactionAsync(cancellationToken);
    }
}