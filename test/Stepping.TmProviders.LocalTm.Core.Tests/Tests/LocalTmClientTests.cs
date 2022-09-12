using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stepping.Core.Databases;
using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.TransactionManagers;
using Xunit;

namespace Stepping.TmProviders.LocalTm.Core.Tests.Tests;

public class LocalTmClientTests : SteppingTmProvidersLocalTmCoreTestBase
{
    protected ITmClient LocalTmClient { get; }
    protected IDistributedJobFactory DistributedJobFactory { get; }
    protected ILocalTmManager LocalTmManager { get; }

    public LocalTmClientTests()
    {
        LocalTmClient = ServiceProvider.GetRequiredService<LocalTmClient>();
        DistributedJobFactory = ServiceProvider.GetRequiredService<IDistributedJobFactory>();
        LocalTmManager = ServiceProvider.GetRequiredService<ILocalTmManager>();
    }

    protected override void ConfigureServices(ServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton(Substitute.For<ILocalTmManager>());
    }

    [Fact]
    public async Task Should_Create_Prepare()
    {
        var job = await DistributedJobFactory.CreateJobAsync(Guid.NewGuid().ToString(), new FakeSteppingDbContext(true, "some-info"));

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        await LocalTmClient.PrepareAsync(job);

        await LocalTmManager.Received().PrepareAsync(
             Arg.Is<string>(x => x == job.Gid),
             Arg.Is<LocalTmStepModel>(x =>
                 x.Steps.Count == 2 &&
                 x.Steps[0].StepName == FakeExecutableStep.FakeExecutableStepName &&
                 x.Steps[1].StepName == FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName),
             Arg.Is<SteppingDbContextLookupInfoModel>(x => x.CustomInfo == "some-info")
        );
    }

    [Fact]
    public async Task Should_Update_Submited()
    {
        var job = await DistributedJobFactory.CreateJobAsync(Guid.NewGuid().ToString(), new FakeSteppingDbContext(true, "some-info"));

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        await LocalTmClient.SubmitAsync(job);

        await LocalTmManager.Received().SubmitAsync(
             Arg.Is<string>(x => x == job.Gid)
        );

        await LocalTmManager.Received().ProcessSubmittedAsync(
             Arg.Is<string>(x => x == job.Gid)
        );
    }
}
