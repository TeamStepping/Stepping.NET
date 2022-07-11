using Microsoft.EntityFrameworkCore;
using Stepping.Core;
using Stepping.Core.Databases;

namespace Stepping.DbProviders.EfCore;

public class EfCoreSteppingDbContext : ISteppingDbContext
{
    public string DbProviderName => SteppingDbProviderEfCoreConsts.DbProviderName;

    public string ConnectionString => DbContext.Database.GetConnectionString() ?? throw new InvalidOperationException();

    public DbContext DbContext { get; }

    public EfCoreSteppingDbContext(DbContext dbContext)
    {
        DbContext = dbContext;
    }
}