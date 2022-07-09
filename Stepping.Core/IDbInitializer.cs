namespace Stepping.Core;

public interface IDbInitializer
{
    Task TryInitializeAsync(IDbInitializingInfoModel infoModel);
}