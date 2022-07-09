using Stepping.Core;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbInitializingInfoModel : IDbInitializingInfoModel
{
    public ISteppingDbContext DbContext { get; }

    public EfCoreDbInitializingInfoModel(EfCoreSteppingDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
}