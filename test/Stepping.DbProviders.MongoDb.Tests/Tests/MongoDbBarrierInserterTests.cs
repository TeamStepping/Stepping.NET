using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shouldly;
using Stepping.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Xunit;

namespace Stepping.DbProviders.MongoDb.Tests.Tests;

[Collection(MongoTestCollection.Name)]
public class MongoDbBarrierInserterTests : SteppingDbProvidersMongoTestBase
{
    protected MongoDbBarrierInserter DbBarrierInserter { get; }

    public MongoDbBarrierInserterTests()
    {
        DbBarrierInserter = ServiceProvider.GetRequiredService<MongoDbBarrierInserter>();
    }

    [Fact]
    public async Task Should_Insert_Barrier()
    {
        var client = new MongoClient(MongoDbFixture.ConnectionString);
        var database = client.GetDatabase(MongoDbTestConsts.Database);
        var steppingDbContext = new MongoDbSteppingDbContext(client, database, null, MongoDbFixture.ConnectionString);

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
        var client = new MongoClient(MongoDbFixture.ConnectionString);
        var database = client.GetDatabase(MongoDbTestConsts.Database);
        var steppingDbContext = new MongoDbSteppingDbContext(client, database, null, MongoDbFixture.ConnectionString);

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
        var client1 = new MongoClient(MongoDbFixture.ConnectionString);
        var database1 = client1.GetDatabase(MongoDbTestConsts.Database);
        var sessionHandle1 = await client1.StartSessionAsync();
        sessionHandle1.StartTransaction();
        var steppingDbContext1 =
            new MongoDbSteppingDbContext(client1, database1, sessionHandle1, MongoDbFixture.ConnectionString);

        var barrierInfoModel = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        var result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext1);

        result.ShouldBeTrue();

        barrierInfoModel.Reason = SteppingConsts.MsgBarrierReasonRollback;

        await using var scope = ServiceProvider.CreateAsyncScope();
        var client2 = new MongoClient(MongoDbFixture.ConnectionString);
        var database2 = client2.GetDatabase(MongoDbTestConsts.Database);
        var steppingDbContext2 =
            new MongoDbSteppingDbContext(client2, database2, null, MongoDbFixture.ConnectionString);

        var task = Task.Run(async () =>
            result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext2));

        await Task.Delay(2000); // Todo: avoid or improve the delay.
        await sessionHandle1.CommitTransactionAsync();

        sessionHandle1.Dispose();

#if NET5_0
        task.Wait(CancellationToken.None);
#else
        await task.WaitAsync(CancellationToken.None);
#endif

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Insert_Rollback_Success_If_Another_Transaction_Rollback()
    {
        var client1 = new MongoClient(MongoDbFixture.ConnectionString);
        var database1 = client1.GetDatabase(MongoDbTestConsts.Database);
        var sessionHandle1 = await client1.StartSessionAsync();
        sessionHandle1.StartTransaction();
        var steppingDbContext1 =
            new MongoDbSteppingDbContext(client1, database1, sessionHandle1, MongoDbFixture.ConnectionString);

        var barrierInfoModel = new BarrierInfoModel(SteppingConsts.TypeMsg, Guid.NewGuid().ToString(),
            SteppingConsts.MsgBranchId, SteppingConsts.MsgOp, SteppingConsts.MsgBarrierId,
            SteppingConsts.MsgBarrierReasonCommit);

        var result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext1);

        result.ShouldBeTrue();

        barrierInfoModel.Reason = SteppingConsts.MsgBarrierReasonRollback;

        await using var scope = ServiceProvider.CreateAsyncScope();
        var client2 = new MongoClient(MongoDbFixture.ConnectionString);
        var database2 = client2.GetDatabase(MongoDbTestConsts.Database);
        var steppingDbContext2 =
            new MongoDbSteppingDbContext(client2, database2, null, MongoDbFixture.ConnectionString);

        var task = Task.Run(async () =>
            result = await DbBarrierInserter.TryInsertBarrierAsync(barrierInfoModel, steppingDbContext2));

        await Task.Delay(2000); // Todo: avoid or improve the delay.
        await sessionHandle1.AbortTransactionAsync();

        sessionHandle1.Dispose();

#if NET5_0
        task.Wait(CancellationToken.None);
#else
        await task.WaitAsync(CancellationToken.None);
#endif

        result.ShouldBeTrue();
    }
}