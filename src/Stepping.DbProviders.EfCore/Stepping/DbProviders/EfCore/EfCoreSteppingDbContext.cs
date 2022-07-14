using Microsoft.EntityFrameworkCore;
using Stepping.Core.Databases;

namespace Stepping.DbProviders.EfCore;

public class EfCoreSteppingDbContext : SteppingDbContextBase
{
    public override string DbProviderName => SteppingDbProviderEfCoreConsts.DbProviderName;

    public DbContext DbContext { get; }

    public override string ConnectionString =>
        DbContext.Database.GetConnectionString() ?? throw new InvalidOperationException();

    public override bool IsTransactional => DbContext.Database.CurrentTransaction is not null;

    public override Type GetInternalDbContextTypeOrNull() => DbContext.GetType();

    public override string? GetInternalDatabaseNameOrNull() => null;

    protected override async Task InternalCommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await DbContext.Database.CommitTransactionAsync(cancellationToken);

    public EfCoreSteppingDbContext(DbContext dbContext)
    {
        DbContext = dbContext;
    }
}