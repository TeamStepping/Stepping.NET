using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Databases;
using Stepping.Core.Infrastructures;

namespace Stepping.DbProviders.EfCore;

public class EfCoreSteppingDbContextProvider : ISteppingDbContextProvider
{
    public string DbProviderName => SteppingDbProviderEfCoreConsts.DbProviderName;

    protected IServiceProvider ServiceProvider { get; }
    protected IConnectionStringEncryptor ConnectionStringEncryptor { get; }

    public EfCoreSteppingDbContextProvider(
        IServiceProvider serviceProvider,
        IConnectionStringEncryptor connectionStringEncryptor)
    {
        ServiceProvider = serviceProvider;
        ConnectionStringEncryptor = connectionStringEncryptor;
    }

    public virtual async Task<ISteppingDbContext> GetAsync(SteppingDbContextInfoModel infoModel)
    {
        var dbContext = (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(
            ServiceProvider,
            Type.GetType(infoModel.DbContextType!)!
        );

        dbContext.Database.SetConnectionString(
            await ConnectionStringEncryptor.DecryptAsync(infoModel.EncryptedConnectionString));

        return new EfCoreSteppingDbContext(dbContext);
    }
}