namespace Stepping.TmProviders.LocalTm.EfCore;

public class TmTransaction
{
    public long Id { get; set; }

    public string Gid { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Steps { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? FinishTime { get; set; }

    public string? RollbackReason { get; set; }

    public DateTime? RollbackTime { get; set; }

    public int? NextRetryInterval { get; set; }

    public DateTime? NextRetryTime { get; set; }

    public string SteppingDbContextLookupInfo { get; set; } = null!;

    public string? ConcurrencyStamp { get; set; }
}
