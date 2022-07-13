using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.TestBase.Fakes;

public class FakeDbBarrierInserter : IDbBarrierInserter
{
    public string DbProviderName => FakeSteppingDbContext.FakeDbProviderName;

    /// <summary>
    /// Gid -> Reason
    /// </summary>
    public Dictionary<string, string> InsertedBarriers { get; } = new();

    public bool ShouldInsertFailed { get; set; }

    public async Task MustInsertBarrierAsync(BarrierInfoModel barrierInfoModel, ISteppingDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (!await TryInsertBarrierAsync(barrierInfoModel, dbContext, cancellationToken))
        {
            throw new DuplicateBarrierException();
        }
    }

    public Task<bool> TryInsertBarrierAsync(BarrierInfoModel barrierInfoModel, ISteppingDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (ShouldInsertFailed || InsertedBarriers.ContainsKey(barrierInfoModel.Gid))
        {
            return Task.FromResult(false);
        }

        InsertedBarriers[barrierInfoModel.Gid] = barrierInfoModel.Reason;

        return Task.FromResult(true);
    }
}