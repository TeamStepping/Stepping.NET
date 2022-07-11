namespace Stepping.Core.Databases;

public interface IBarrierInfoModelFactory
{
    Task<BarrierInfoModel> CreateForCommitAsync(string gid);

    Task<BarrierInfoModel> CreateForRollbackAsync(string gid);
}