namespace Stepping.Core.Databases;

public interface ISteppingDbContextLookupInfoProvider
{
    Task<SteppingDbContextLookupInfoModel> GetAsync(ISteppingDbContext dbContext);
}