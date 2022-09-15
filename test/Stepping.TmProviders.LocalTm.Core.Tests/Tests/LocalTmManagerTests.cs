using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.Core.Jobs;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.Timing;
using Stepping.TmProviders.LocalTm.TransactionManagers;
using Xunit;

namespace Stepping.TmProviders.LocalTm.Core.Tests.Tests;

public class LocalTmManagerTests : SteppingTmProvidersLocalTmCoreTestBase
{
    protected ILocalTmManager LocalTmManager { get; }
    protected ITransactionStore TransactionStore { get; }
    protected ISteppingClock SteppingClock { get; }
    protected IDistributedJobFactory DistributedJobFactory { get; }
    protected ILocalTmStepConverter LocalTmStepConverter { get; }
    protected ISteppingDbContextLookupInfoProvider SteppingDbContextLookupInfoProvider { get; }
    protected IDbBarrierInserter DbBarrierInserter { get; }
    protected IBarrierInfoModelFactory BarrierInfoModelFactory { get; }

    public LocalTmManagerTests()
    {
        LocalTmManager = ServiceProvider.GetRequiredService<LocalTmManager>();
        TransactionStore = ServiceProvider.GetRequiredService<ITransactionStore>();
        SteppingClock = ServiceProvider.GetRequiredService<ISteppingClock>();
        DistributedJobFactory = ServiceProvider.GetRequiredService<IDistributedJobFactory>();
        LocalTmStepConverter = ServiceProvider.GetRequiredService<ILocalTmStepConverter>();
        SteppingDbContextLookupInfoProvider = ServiceProvider.GetRequiredService<ISteppingDbContextLookupInfoProvider>();
        DbBarrierInserter = ServiceProvider.GetRequiredService<IDbBarrierInserter>();
        BarrierInfoModelFactory = ServiceProvider.GetRequiredService<IBarrierInfoModelFactory>();
    }

    [Fact]
    public async Task Should_Create_Prepare()
    {
        var (gid, _, _) = await PrepareAsync();

        var model = await TransactionStore.GetAsync(gid);
        model.Gid.ShouldBe(gid);
        model.Status.ShouldBe(LocalTmConst.StatusPrepare);
    }

    [Fact]
    public async Task Should_Update_Submit()
    {
        var (gid, stepModel, _) = await PrepareAsync();

        await LocalTmManager.SubmitAsync(gid, stepModel);

        var model = await TransactionStore.GetAsync(gid);
        model.Gid.ShouldBe(gid);
        model.Status.ShouldBe(LocalTmConst.StatusSubmit);
    }

    [Fact]
    public async Task Should_Create_Submit()
    {
        var gid = Guid.NewGuid().ToString();

        var job = await DistributedJobFactory.CreateJobAsync();
        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));
        var stepModel = await LocalTmStepConverter.ConvertAsync(job.Steps);

        await LocalTmManager.SubmitAsync(gid, stepModel);

        var model = await TransactionStore.GetAsync(gid);
        model.Gid.ShouldBe(gid);
        model.Status.ShouldBe(LocalTmConst.StatusSubmit);
    }

    [Fact]
    public async Task Should_Not_Submit_If_Status_Is_Not_Prepare()
    {
        var (gid, stepModel, _) = await PrepareAsync();

        var existModel = await TransactionStore.GetAsync(gid);
        existModel.Status = LocalTmConst.StatusRollback;
        existModel.UpdateTime = SteppingClock.Now;

        await TransactionStore.UpdateAsync(existModel);

        await LocalTmManager.SubmitAsync(gid, stepModel);

        var model = await TransactionStore.GetAsync(gid);
        model.Status.ShouldBe(LocalTmConst.StatusRollback);
        model.UpdateTime.ShouldBe(existModel.UpdateTime);
    }

    [Fact]
    public async Task Should_ProcessPending()
    {
        var (gid1, _, _) = await PrepareAsync();
        var tmModel1 = await TransactionStore.GetAsync(gid1);
        tmModel1.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel1);

        var (gid2, stepModel2, _) = await PrepareAsync();
        await LocalTmManager.SubmitAsync(gid2, stepModel2);
        var tmModel2 = await TransactionStore.GetAsync(gid2);
        tmModel2.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel2);

        var (gid3, _, _) = await PrepareAsync(null, false);

        var tmModel3 = await TransactionStore.GetAsync(gid3);
        tmModel3.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel3);

        var gid4 = Guid.NewGuid().ToString();

        var job = await DistributedJobFactory.CreateJobAsync(gid4);
        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));
        var stepModel4 = await LocalTmStepConverter.ConvertAsync(job.Steps);
        await LocalTmManager.SubmitAsync(gid4, stepModel4);
        var tmModel4 = await TransactionStore.GetAsync(gid4);
        tmModel4.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel4);

        await LocalTmManager.ProcessPendingAsync();

        var exitsTmModel1 = await TransactionStore.GetAsync(gid1);
        exitsTmModel1.Status.ShouldBe(LocalTmConst.StatusFinish);
        var exitsTmModel2 = await TransactionStore.GetAsync(gid2);
        exitsTmModel2.Status.ShouldBe(LocalTmConst.StatusFinish);
        var exitsTmModel3 = await TransactionStore.GetAsync(gid3);
        exitsTmModel3.Status.ShouldBe(LocalTmConst.StatusRollback);
        var exitsTmModel4 = await TransactionStore.GetAsync(gid4);
        exitsTmModel4.Status.ShouldBe(LocalTmConst.StatusFinish);
    }

    [Fact]
    public async Task Should_ProcessSubmitted()
    {
        var (gid, stepModel, _) = await PrepareAsync(job =>
        {
            job.AddStep(new RequestGitHubGetRepoStep("TeamStepping", "Stepping.NET"));
        });

        await LocalTmManager.SubmitAsync(gid, stepModel);

        await LocalTmManager.ProcessSubmittedAsync(gid);

        var existModel = await TransactionStore.GetAsync(gid);
        existModel.Gid.ShouldBe(gid);
        existModel.Status.ShouldBe(LocalTmConst.StatusFinish);
        existModel.Steps.Steps.Count.ShouldBe(3);
        existModel.Steps.Steps[0].Executed.ShouldBe(true);
        existModel.Steps.Steps[1].Executed.ShouldBe(true);
        existModel.Steps.Steps[2].Executed.ShouldBe(true);
    }

    [Fact]
    public async Task Should_Retry_If_Steps_Exectue_Failed()
    {
        var (gid, stepModel, _) = await PrepareAsync(job =>
        {
            job.AddStep<FakeControlExecutableStep>();
        });

        await LocalTmManager.SubmitAsync(gid, stepModel);

        FakeControlExecutableStep.Throw = true;

        await LocalTmManager.ProcessSubmittedAsync(gid);

        var existModel = await TransactionStore.GetAsync(gid);
        existModel.Gid.ShouldBe(gid);
        existModel.Status.ShouldBe(LocalTmConst.StatusSubmit);
        existModel.NextRetryInterval.ShouldBe(1);
        existModel.NextRetryTime.ShouldNotBeNull();
        existModel.Steps.Steps.Count.ShouldBe(3);
        existModel.Steps.Steps[0].Executed.ShouldBe(true);
        existModel.Steps.Steps[1].Executed.ShouldBe(true);
        existModel.Steps.Steps[2].Executed.ShouldBe(false);
    }

    [Fact]
    public async Task Should_Not_ProcessSubmitted_If_Status_Is_Not_Submit()
    {
        var (gid, _, _) = await PrepareAsync();

        var existModel = await TransactionStore.GetAsync(gid);

        await LocalTmManager.ProcessSubmittedAsync(gid);

        var model = await TransactionStore.GetAsync(gid);
        model.Status.ShouldBe(LocalTmConst.StatusPrepare);
        model.UpdateTime.ShouldBe(existModel.UpdateTime);
    }

    protected async Task<(string gid, LocalTmStepModel localTmStepModel, SteppingDbContextLookupInfoModel dbContextLookupInfoModel)> PrepareAsync(
        Action<IDistributedJob>? setupDistributedJob = null,
        bool insertBarrier = true)
    {
        var gid = Guid.NewGuid().ToString();

        var job = await DistributedJobFactory.CreateJobAsync(gid, new FakeSteppingDbContext(true, "some-info"));
        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        setupDistributedJob?.Invoke(job);

        var steppingDbContextLookupInfo = await SteppingDbContextLookupInfoProvider.GetAsync(job.DbContext!);

        if (insertBarrier)
        {
            await DbBarrierInserter.MustInsertBarrierAsync(await BarrierInfoModelFactory.CreateForCommitAsync(gid), job.DbContext!);
        }

        var stepModel = await LocalTmStepConverter.ConvertAsync(job.Steps);

        await LocalTmManager.PrepareAsync(gid, stepModel, steppingDbContextLookupInfo);

        return (gid, stepModel, steppingDbContextLookupInfo);
    }
}
