namespace Stepping.Core.Databases;

public class BarrierInfoModelFactory : IBarrierInfoModelFactory
{
    public virtual Task<BarrierInfoModel> CreateForCommitAsync(string gid)
    {
        return Task.FromResult(new BarrierInfoModel(
            SteppingConsts.TypeMsg,
            gid,
            SteppingConsts.MsgBranchId,
            SteppingConsts.MsgOp,
            SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit));
    }

    public virtual Task<BarrierInfoModel> CreateForRollbackAsync(string gid)
    {
        return Task.FromResult(new BarrierInfoModel(
            SteppingConsts.TypeMsg,
            gid,
            SteppingConsts.MsgBranchId,
            SteppingConsts.MsgOp,
            SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonRollback));
    }
}