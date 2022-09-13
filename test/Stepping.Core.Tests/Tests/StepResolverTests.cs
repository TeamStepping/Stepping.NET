using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class StepResolverTests : SteppingCoreTestBase
{
    protected IStepResolver StepResolver { get; }
    protected IStepArgsSerializer StepArgsSerializer { get; }

    public StepResolverTests()
    {
        StepResolver = ServiceProvider.GetRequiredService<IStepResolver>();
        StepArgsSerializer = ServiceProvider.GetRequiredService<IStepArgsSerializer>();
    }

    [Fact]
    public Task Should_Resolve_Step()
    {
        var fakeStep = StepResolver.Resolve(FakeExecutableStep.FakeExecutableStepName);
        var fakeArgsStep = StepResolver.Resolve(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName,
            new FakeArgs("my-input"));

        fakeStep.GetType().ShouldBe(typeof(FakeExecutableStep));
        fakeArgsStep.GetType().ShouldBe(typeof(FakeWithArgsExecutableStep));
        ((FakeWithArgsExecutableStep)fakeArgsStep).Args.Name.ShouldBe("my-input");

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_Resolve_Step_Args()
    {
        var args = await StepResolver.ResolveArgsAsync(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName,
            Encoding.UTF8.GetString(await StepArgsSerializer.SerializeAsync(new FakeArgs("my-input"))));

        args.ShouldNotBeNull();
        args.GetType().ShouldBe(typeof(FakeArgs));
        ((FakeArgs)args).Name.ShouldBe("my-input");
    }

    [Fact]
    public async Task Should_Resolve_Step_By_Specified_Type()
    {
        const string endpoint = "https://api.github.com/orgs/TeamStepping";

        var args = await StepResolver.ResolveArgsAsync(
            RequestGitHubGetOrganizationStep.RequestGitHubGetOrganizationStepName,
            Encoding.UTF8.GetString(await StepArgsSerializer.SerializeAsync(
                new HttpRequestStepArgs(endpoint, HttpMethod.Get)))
        );

        args.ShouldNotBeNull();
        args.GetType().ShouldBe(typeof(HttpRequestStepArgs));
        ((HttpRequestStepArgs)args).Endpoint.ShouldBe(endpoint);
    }
}