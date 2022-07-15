using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class StepResolverTests : SteppingCoreTestBase
{
    protected IStepResolver StepResolver { get; }

    public StepResolverTests()
    {
        StepResolver = ServiceProvider.GetRequiredService<IStepResolver>();
    }

    [Fact]
    public Task Should_Resolve_Step()
    {
        var fakeStep = StepResolver.Resolve(FakeExecutableStep.FakeExecutableStepName);
        var fakeArgsStep = StepResolver.Resolve(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName, "my-input");

        fakeStep.GetType().ShouldBe(typeof(FakeExecutableStep));
        fakeArgsStep.GetType().ShouldBe(typeof(FakeWithArgsExecutableStep));

        return Task.CompletedTask;
    }
}