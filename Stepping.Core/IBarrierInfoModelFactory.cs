namespace Stepping.Core;

public interface IBarrierInfoModelFactory
{
    Task<BarrierInfoModel> CreateForCommitAsync(IDistributedJob job);

    Task<BarrierInfoModel> CreateForRollbackAsync(IDistributedJob job);
}