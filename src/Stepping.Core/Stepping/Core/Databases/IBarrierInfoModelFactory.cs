using Stepping.Core.Jobs;

namespace Stepping.Core.Databases;

public interface IBarrierInfoModelFactory
{
    Task<BarrierInfoModel> CreateForCommitAsync(IDistributedJob job);

    Task<BarrierInfoModel> CreateForRollbackAsync(IDistributedJob job);
}