namespace Stepping.Core;

public class BarrierInfoModel
{
    public string TransType { get; set; } = null!;

    public string Gid { get; set; } = null!;

    public string BranchId { get; set; } = null!;

    public string Op { get; set; } = null!;

    public string BarrierId { get; set; } = null!;

    public string Reason { get; set; } = null!;

    protected BarrierInfoModel()
    {
    }

    public BarrierInfoModel(string transType, string gid, string branchId, string op, string barrierId, string reason)
    {
        TransType = transType;
        Gid = gid;
        BranchId = branchId;
        Op = op;
        BarrierId = barrierId;
        Reason = reason;
    }
}