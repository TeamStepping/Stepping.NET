namespace Stepping.Core.Databases;

public interface ISteppingDbContextProvider
{
    string DbProviderName { get; }

    Task<ISteppingDbContext> GetAsync(SteppingDbContextLookupInfoModel infoModel);
}