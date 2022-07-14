using Stepping.Core.Databases;

namespace Stepping.TestBase.Fakes;

public class FakeSteppingDbContextProvider : ISteppingDbContextProvider
{
    public string DbProviderName => FakeSteppingDbContext.FakeDbProviderName;

    public Task<ISteppingDbContext> GetAsync(SteppingDbContextInfoModel infoModel)
    {
        return Task.FromResult<ISteppingDbContext>(new FakeSteppingDbContext(false));
    }
}