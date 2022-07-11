namespace Stepping.Core.Databases;

public interface IDbBarrierInserter
{
    string DbProviderName { get; }

    Task MustInsertBarrierAsync(BarrierInfoModel barrierInfoModel, ISteppingDbContext dbContext,
        CancellationToken cancellationToken = default);

    Task<bool> TryInsertBarrierAsync(BarrierInfoModel barrierInfoModel, ISteppingDbContext dbContext,
        CancellationToken cancellationToken = default);
}