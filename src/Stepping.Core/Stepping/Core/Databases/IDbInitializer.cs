namespace Stepping.Core.Databases;

public interface IDbInitializer
{
    Task TryInitializeAsync(IDbInitializingInfoModel infoModel);
}