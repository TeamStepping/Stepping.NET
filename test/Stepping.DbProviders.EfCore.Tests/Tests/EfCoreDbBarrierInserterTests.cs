using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.DbProviders.EfCore.Tests.Fakes;
using Xunit;

namespace Stepping.DbProviders.EfCore.Tests.Tests;

public class EfCoreDbBarrierInserterTests : SteppingDbProvidersEfCoreTestBase
{
    protected EfCoreDbBarrierInserter DbBarrierInserter { get; }

    public EfCoreDbBarrierInserterTests()
    {
        DbBarrierInserter = ServiceProvider.GetRequiredService<EfCoreDbBarrierInserter>();
    }

    [Fact]
    public async Task Should_Insert_Barrier()
    {
        var dbContext = ServiceProvider.GetRequiredService<FakeDbContext>();
        var steppingDbContext = new EfCoreSteppingDbContext(dbContext);

        var barrierInfoModel1 = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        var result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel1, steppingDbContext);

        result.ShouldBeTrue();

        var barrierInfoModel2 = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        await Should.NotThrowAsync(DbBarrierInserter.MustInsertBarrierAsync(barrierInfoModel2, steppingDbContext));
    }

    [Fact]
    public async Task Should_Not_Insert_Barrier_If_Duplicate()
    {
        var dbContext = ServiceProvider.GetRequiredService<FakeDbContext>();
        var steppingDbContext = new EfCoreSteppingDbContext(dbContext);

        var barrierInfoModel = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        var result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext);

        result.ShouldBeTrue();

        barrierInfoModel.Reason = SteppingConsts.MsgBarrierReasonRollback;

        result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext);

        result.ShouldBeFalse();

        await Should.ThrowAsync<DuplicateBarrierException>(
            DbBarrierInserter.MustInsertBarrierAsync(barrierInfoModel, steppingDbContext));
    }

    [Fact]
    public async Task Should_Insert_Rollback_Success_If_Another_Transaction_Commit()
    {
        var dbContext1 = ServiceProvider.GetRequiredService<FakeSharedDbContext>();
        var steppingDbContext1 = new EfCoreSteppingDbContext(dbContext1);
        await InitializeBarrierTableAsync(steppingDbContext1);

        var transaction1 = await dbContext1.Database.BeginTransactionAsync();

        var barrierInfoModel = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        var result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext1);

        result.ShouldBeTrue();

        barrierInfoModel.Reason = SteppingConsts.MsgBarrierReasonRollback;

        var task = Task.Run(async () =>
        {
            await using var scope = ServiceProvider.CreateAsyncScope();
            var dbContext2 = scope.ServiceProvider.GetRequiredService<FakeSharedDbContext>();
            var steppingDbContext2 = new EfCoreSteppingDbContext(dbContext2);

            // Todo: sometimes throw `System.ArgumentOutOfRangeException Specified argument was out of the range of valid values. at SQLitePCL.SQLite3Provider_e_sqlite3.SQLitePCL.ISQLite3Provider.sqlite3_prepare_v2(sqlite3 db, ReadOnlySpan`1 sql, IntPtr& stm, ReadOnlySpan`1& tail)`
            result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext2);
        });

        await Task.Delay(2000); // Todo: avoid or improve the delay.
        await transaction1.CommitAsync();

        await transaction1.DisposeAsync();
        await dbContext1.DisposeAsync();

#if NETCOREAPP3_1
        task.Wait(CancellationToken.None);
#else
        await task.WaitAsync(CancellationToken.None);
#endif

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Insert_Rollback_Success_If_Another_Transaction_Rollback()
    {
        var dbContext1 = ServiceProvider.GetRequiredService<FakeSharedDbContext>();
        var steppingDbContext1 = new EfCoreSteppingDbContext(dbContext1);
        await InitializeBarrierTableAsync(steppingDbContext1);

        var transaction1 = await dbContext1.Database.BeginTransactionAsync();

        var barrierInfoModel = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        var result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext1);

        result.ShouldBeTrue();

        barrierInfoModel.Reason = SteppingConsts.MsgBarrierReasonRollback;

        var task = Task.Run(async () =>
        {
            await using var scope = ServiceProvider.CreateAsyncScope();
            var dbContext2 = scope.ServiceProvider.GetRequiredService<FakeSharedDbContext>();
            var steppingDbContext2 = new EfCoreSteppingDbContext(dbContext2);

            // Todo: sometimes throw `System.ArgumentOutOfRangeException Specified argument was out of the range of valid values. at SQLitePCL.SQLite3Provider_e_sqlite3.SQLitePCL.ISQLite3Provider.sqlite3_prepare_v2(sqlite3 db, ReadOnlySpan`1 sql, IntPtr& stm, ReadOnlySpan`1& tail)`
            result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext2);
        });

        await Task.Delay(2000); // Todo: avoid or improve the delay.
        await transaction1.RollbackAsync();

        await transaction1.DisposeAsync();
        await dbContext1.DisposeAsync();

#if NETCOREAPP3_1
        task.Wait(CancellationToken.None);
#else
        await task.WaitAsync(CancellationToken.None);
#endif

        result.ShouldBeTrue();
    }

    private async Task InitializeBarrierTableAsync(EfCoreSteppingDbContext efCoreSteppingDbContext)
    {
        var dbInitializer = ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.TryInitializeAsync(new EfCoreDbInitializingInfoModel(efCoreSteppingDbContext));
    }
}