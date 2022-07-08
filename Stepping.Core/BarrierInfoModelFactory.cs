namespace Stepping.Core;

public class BarrierInfoModelFactory : IBarrierInfoModelFactory
{
    public virtual Task<BarrierInfoModel> CreateForCommitAsync(IDistributedJob job)
    {
        return Task.FromResult(new BarrierInfoModel(
            SteppingConsts.TypeMsg,
            job.Gid,
            SteppingConsts.MsgBranchId,
            SteppingConsts.MsgOp,
            SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit));
    }

    public virtual Task<BarrierInfoModel> CreateForRollbackAsync(IDistributedJob job)
    {
        return Task.FromResult(new BarrierInfoModel(
            SteppingConsts.TypeMsg,
            job.Gid,
            SteppingConsts.MsgBranchId,
            SteppingConsts.MsgOp,
            SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonRollback));
    }
}