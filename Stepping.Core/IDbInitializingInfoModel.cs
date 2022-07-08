namespace Stepping.Core;

public interface IDbInitializingInfoModel
{
    ISteppingDbContext DbContext { get; }
}