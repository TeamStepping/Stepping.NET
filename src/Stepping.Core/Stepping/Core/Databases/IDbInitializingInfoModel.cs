namespace Stepping.Core.Databases;

public interface IDbInitializingInfoModel
{
    ISteppingDbContext DbContext { get; }
}