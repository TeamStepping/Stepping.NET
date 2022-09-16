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
        var job = await PrepareAsync();

        var model = await TransactionStore.GetAsync(job.Gid);
        model.Gid.ShouldBe(job.Gid);
        model.Status.ShouldBe(LocalTmConst.StatusPrepare);
    }

    [Fact]
    public async Task Should_Update_Submit()
    {
        var job = await PrepareAsync();

        await LocalTmManager.SubmitAsync(job);

        var model = await TransactionStore.GetAsync(job.Gid);
        model.Gid.ShouldBe(job.Gid);
        model.Status.ShouldBe(LocalTmConst.StatusSubmit);
    }

    [Fact]
    public async Task Should_Create_Submit()
    {
        var job = await DistributedJobFactory.CreateJobAsync();
        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        await LocalTmManager.SubmitAsync(job);

        var model = await TransactionStore.GetAsync(job.Gid);
        model.Gid.ShouldBe(job.Gid);
        model.Status.ShouldBe(LocalTmConst.StatusSubmit);
    }

    [Fact]
    public async Task Should_Not_Submit_If_Status_Is_Not_Prepare()
    {
        var job = await PrepareAsync();

        var existModel = await TransactionStore.GetAsync(job.Gid);
        existModel.Status = LocalTmConst.StatusRollback;
        existModel.UpdateTime = SteppingClock.Now;

        await TransactionStore.UpdateAsync(existModel);

        await LocalTmManager.SubmitAsync(job);

        var model = await TransactionStore.GetAsync(job.Gid);
        model.Status.ShouldBe(LocalTmConst.StatusRollback);
        model.UpdateTime.ShouldBe(existModel.UpdateTime);
    }

    [Fact]
    public async Task Should_ProcessPending()
    {
        var job1 = await PrepareAsync();
        var tmModel1 = await TransactionStore.GetAsync(job1.Gid);
        tmModel1.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel1);

        var job2 = await PrepareAsync();
        await LocalTmManager.SubmitAsync(job2);
        var tmModel2 = await TransactionStore.GetAsync(job2.Gid);
        tmModel2.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel2);

        var job3 = await PrepareAsync(null, false);
        var tmModel3 = await TransactionStore.GetAsync(job3.Gid);
        tmModel3.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel3);

        var job4 = await PrepareAsync(null, false);
        await LocalTmManager.SubmitAsync(job4);
        var tmModel4 = await TransactionStore.GetAsync(job4.Gid);
        tmModel4.CreationTime = SteppingClock.Now.AddMinutes(-2);
        await TransactionStore.UpdateAsync(tmModel4);

        var job5 = await PrepareAsync(null, false);
        await LocalTmManager.SubmitAsync(job5);
        var tmModel5 = await TransactionStore.GetAsync(job5.Gid);
        tmModel5.NextRetryInterval = 1;
        tmModel5.NextRetryTime = SteppingClock.Now;
        await TransactionStore.UpdateAsync(tmModel5);

        await LocalTmManager.ProcessPendingAsync();

        var exitsTmModel1 = await TransactionStore.GetAsync(job1.Gid);
        exitsTmModel1.Status.ShouldBe(LocalTmConst.StatusFinish);
        var exitsTmModel2 = await TransactionStore.GetAsync(job2.Gid);
        exitsTmModel2.Status.ShouldBe(LocalTmConst.StatusFinish);
        var exitsTmModel3 = await TransactionStore.GetAsync(job3.Gid);
        exitsTmModel3.Status.ShouldBe(LocalTmConst.StatusRollback);
        var exitsTmModel4 = await TransactionStore.GetAsync(job4.Gid);
        exitsTmModel4.Status.ShouldBe(LocalTmConst.StatusFinish);
        var exitsTmModel5 = await TransactionStore.GetAsync(job5.Gid);
        exitsTmModel5.Status.ShouldBe(LocalTmConst.StatusFinish);
    }

    [Fact]
    public async Task Should_ProcessSubmitted()
    {
        var job = await PrepareAsync(job =>
        {
            job.AddStep(new RequestGitHubGetRepoStep("TeamStepping", "Stepping.NET"));
        });

        await LocalTmManager.SubmitAsync(job);

        await LocalTmManager.ProcessSubmittedAsync(job);

        var existModel = await TransactionStore.GetAsync(job.Gid);
        existModel.Gid.ShouldBe(job.Gid);
        existModel.Status.ShouldBe(LocalTmConst.StatusFinish);
        existModel.Steps.Steps.Count.ShouldBe(3);
        existModel.Steps.Steps[0].Executed.ShouldBe(true);
        existModel.Steps.Steps[1].Executed.ShouldBe(true);
        existModel.Steps.Steps[2].Executed.ShouldBe(true);
    }

    [Fact]
    public async Task Should_Retry_If_Steps_Exectue_Failed()
    {
        var job = await PrepareAsync(job =>
        {
            job.AddStep<FakeControlExecutableStep>();
        });

        await LocalTmManager.SubmitAsync(job);

        FakeControlExecutableStep.Throw = true;

        await LocalTmManager.ProcessSubmittedAsync(job);

        var existModel = await TransactionStore.GetAsync(job.Gid);
        existModel.Gid.ShouldBe(job.Gid);
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
        var job = await PrepareAsync();

        var existModel = await TransactionStore.GetAsync(job.Gid);

        await LocalTmManager.ProcessSubmittedAsync(job);

        var model = await TransactionStore.GetAsync(job.Gid);
        model.Status.ShouldBe(LocalTmConst.StatusPrepare);
        model.UpdateTime.ShouldBe(existModel.UpdateTime);
    }

    protected async Task<IDistributedJob> PrepareAsync(
        Action<IDistributedJob>? setupDistributedJob = null,
        bool insertBarrier = true)
    {
        var job = await DistributedJobFactory.CreateJobAsync(Guid.NewGuid().ToString(), new FakeSteppingDbContext(true, "some-info"));
        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        setupDistributedJob?.Invoke(job);

        if (insertBarrier)
        {
            await DbBarrierInserter.MustInsertBarrierAsync(await BarrierInfoModelFactory.CreateForCommitAsync(job.Gid), job.DbContext!);
        }

        await LocalTmManager.PrepareAsync(job);

        return job;
    }
}
