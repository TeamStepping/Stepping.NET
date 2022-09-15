using Stepping.Core.Databases;
using Stepping.TmProviders.LocalTm.Steps;

namespace Stepping.TmProviders.LocalTm.Models;

public class TmTransactionModel
{
    public string Gid { get; set; } = null!;

    public string Status { get; set; } = null!;

    public LocalTmStepModel Steps { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? FinishTime { get; set; }

    public string? RollbackReason { get; set; }

    public DateTime? RollbackTime { get; set; }

    public int? NextRetryInterval { get; set; }

    public DateTime? NextRetryTime { get; set; }

    public SteppingDbContextLookupInfoModel? SteppingDbContextLookupInfo { get; set; }

    public string? ConcurrencyStamp { get; set; }

    protected TmTransactionModel() { }

    public TmTransactionModel(string gid, LocalTmStepModel steps, SteppingDbContextLookupInfoModel? steppingDbContextLookupInfo, DateTime creationTime)
    {
        Gid = gid;
        Status = LocalTmConst.StatusPrepare;
        Steps = steps;
        SteppingDbContextLookupInfo = steppingDbContextLookupInfo;
        CreationTime = creationTime;
    }

    public void CalculateNextRetryTime(DateTime now)
    {
        NextRetryInterval ??= 0;

        NextRetryInterval++;

        NextRetryTime = now.AddSeconds(Math.Pow(2, NextRetryInterval!.Value));
    }
}
