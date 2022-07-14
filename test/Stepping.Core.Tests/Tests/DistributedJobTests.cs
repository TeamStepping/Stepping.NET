using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Exceptions;
using Stepping.Core.Jobs;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class DistributedJobTests : SteppingCoreTestBase
{
    protected IDistributedJobFactory DistributedJobFactory { get; }

    public DistributedJobTests()
    {
        DistributedJobFactory = ServiceProvider.GetRequiredService<IDistributedJobFactory>();
    }

    [Fact]
    public async Task Should_Add_Steps()
    {
        var job = await DistributedJobFactory.CreateJobAsync("my-gid", null);

        // Executable step
        Should.NotThrow(() => job.AddStep<FakeExecutableStep>());

        // Duplicate step
        Should.NotThrow(() => job.AddStep<FakeExecutableStep>());

        // Executable step with args
        Should.NotThrow(() =>
            job.AddStep<FakeWithArgsExecutableStep, TargetServiceInfoArgs>(
                new TargetServiceInfoArgs(typeof(FakeService))));

        job.Steps.Count.ShouldBe(3);
        job.Steps[0].StepName.ShouldBe(FakeExecutableStep.FakeExecutableStepName);
        job.Steps[1].StepName.ShouldBe(FakeExecutableStep.FakeExecutableStepName);
        job.Steps[2].StepName.ShouldBe(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName);
    }

    [Fact]
    public async Task Should_Not_Send_Prepare_And_Insert_Barrier_Without_Db_Transaction()
    {
        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", null);

        job.AddStep<FakeExecutableStep>();

        await Should.ThrowAsync<SteppingException>(() => job.PrepareAndInsertBarrierAsync(), "DB Transaction not set.");
    }

    [Fact]
    public async Task Should_Send_Prepare_And_Insert_Barrier()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", dbContext);

        job.AddStep<FakeExecutableStep>();

        await Should.NotThrowAsync(() => job.PrepareAndInsertBarrierAsync());
        job.PrepareSent.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Submit_Job_Without_A_Step()
    {
        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", null);

        await Should.ThrowAsync<SteppingException>(() => job.SubmitAsync(), "Steps not set.");
    }

    [Fact]
    public async Task Should_Submit_Job()
    {
        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", null);

        job.AddStep<FakeExecutableStep>();

        await Should.NotThrowAsync(() => job.SubmitAsync());
        job.PrepareSent.ShouldBeFalse();
        job.SubmitSent.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Submit_Job_After_Db_Transaction_Commit()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", dbContext);

        job.AddStep<FakeExecutableStep>();

        await job.PrepareAndInsertBarrierAsync();
        job.PrepareSent.ShouldBeTrue();

        await dbContext.CommitTransactionAsync();
        dbContext.TransactionCommitted.ShouldBeTrue();

        await Should.NotThrowAsync(() => job.SubmitAsync());
        job.SubmitSent.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Execute_Job_Without_Db_Transaction()
    {
        var job = await DistributedJobFactory.CreateJobAsync("my-gid", null);

        job.AddStep<FakeExecutableStep>();

        job.PrepareSent.ShouldBeFalse();
        job.SubmitSent.ShouldBeFalse();

        await Should.NotThrowAsync(() => job.ExecuteAsync());
        job.PrepareSent.ShouldBeFalse();
        job.SubmitSent.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Execute_Job_With_Db_Transaction()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateJobAsync("my-gid", dbContext);

        job.AddStep<FakeExecutableStep>();

        dbContext.TransactionCommitted.ShouldBeFalse();
        job.PrepareSent.ShouldBeFalse();
        job.SubmitSent.ShouldBeFalse();

        await Should.NotThrowAsync(() => job.ExecuteAsync());
        dbContext.TransactionCommitted.ShouldBeTrue();
        job.PrepareSent.ShouldBeTrue();
        job.SubmitSent.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Allow_Duplicate_Sending_Prepare()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", dbContext);

        job.AddStep<FakeExecutableStep>();

        await Should.NotThrowAsync(() => job.PrepareAndInsertBarrierAsync());
        await Should.ThrowAsync<SteppingException>(() => job.PrepareAndInsertBarrierAsync(),
            "Duplicate sending prepare to TM.");
    }

    [Fact]
    public async Task Should_Not_Allow_Duplicate_Sending_Submit()
    {
        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", null);

        job.AddStep<FakeExecutableStep>();

        await Should.NotThrowAsync(() => job.SubmitAsync());
        await Should.ThrowAsync<SteppingException>(() => job.SubmitAsync(), "Duplicate sending submit to TM.");
    }

    [Fact]
    public async Task Should_Not_Allow_Duplicate_Invoking_ExecuteAsync()
    {
        var job = await DistributedJobFactory.CreateJobAsync("my-gid", null);

        job.AddStep<FakeExecutableStep>();

        await Should.NotThrowAsync(() => job.ExecuteAsync());
        await Should.ThrowAsync<SteppingException>(() => job.ExecuteAsync(), "Duplicate sending submit to TM.");

        var dbContext = new FakeSteppingDbContext(true);

        var transJob = await DistributedJobFactory.CreateJobAsync("my-gid", dbContext);

        transJob.AddStep<FakeExecutableStep>();

        await Should.NotThrowAsync(() => transJob.ExecuteAsync());
        await Should.ThrowAsync<SteppingException>(() => transJob.ExecuteAsync(), "Duplicate sending prepare to TM.");
    }
}