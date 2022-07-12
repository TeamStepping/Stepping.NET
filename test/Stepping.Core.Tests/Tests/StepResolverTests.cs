using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.Core.Tests.Fakes;
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
    public async Task Should_Resolve_Step()
    {
        var fakeStep = await StepResolver.ResolveAsync(FakeExecutableStep.FakeExecutableStepName);
        var fakeArgsStep = await StepResolver.ResolveAsync(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName);

        fakeStep.GetType().ShouldBe(typeof(FakeExecutableStep));
        fakeArgsStep.GetType().ShouldBe(typeof(FakeWithArgsExecutableStep));
    }
}