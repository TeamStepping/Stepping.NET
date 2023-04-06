using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stepping.Core.Databases;
using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.TransactionManagers;
using Xunit;

namespace Stepping.TmProviders.LocalTm.Core.Tests.Tests;

public class LocalTmClientTests : SteppingTmProvidersLocalTmCoreTestBase
{
    protected ITmClient LocalTmClient { get; }
    protected IAtomicJobFactory AtomicJobFactory { get; }
    protected ILocalTmManager LocalTmManager { get; }

    public LocalTmClientTests()
    {
        LocalTmClient = ServiceProvider.GetRequiredService<LocalTmClient>();
        AtomicJobFactory = ServiceProvider.GetRequiredService<IAtomicJobFactory>();
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
        var job = await AtomicJobFactory.CreateJobAsync(Guid.NewGuid().ToString(), new FakeSteppingDbContext(true, "some-info"));

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        await LocalTmClient.PrepareAsync(job);

        await LocalTmManager.Received().PrepareAsync(
             Arg.Is<IAtomicJob>(x => x == job)
        );
    }

    [Fact]
    public async Task Should_Update_Submited()
    {
        var job = await AtomicJobFactory.CreateJobAsync(Guid.NewGuid().ToString(), new FakeSteppingDbContext(true, "some-info"));

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        await LocalTmClient.SubmitAsync(job);

        await LocalTmManager.Received().SubmitAsync(
             Arg.Is<IAtomicJob>(x => x == job)
        );

        await LocalTmManager.Received().ProcessSubmittedAsync(
             Arg.Is<IAtomicJob>(x => x == job)
        );
    }

    [Fact]
    public async Task Should_Create_Submited()
    {
        var job = await AtomicJobFactory.CreateJobAsync();

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        await LocalTmClient.SubmitAsync(job);

        await LocalTmManager.Received().SubmitAsync(
             Arg.Is<IAtomicJob>(x => x == job)
        );

        await LocalTmManager.Received().ProcessSubmittedAsync(
             Arg.Is<IAtomicJob>(x => x == job)
        );
    }
}
