using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class StepNameProviderTests : SteppingCoreTestBase
{
    protected IStepNameProvider StepNameProvider { get; }

    public StepNameProviderTests()
    {
        StepNameProvider = ServiceProvider.GetRequiredService<IStepNameProvider>();
    }

    [Fact]
    public Task Should_Get_Step_Name()
    {
        StepNameProvider.Get<FakeExecutableStep>().ShouldBe(FakeExecutableStep.FakeExecutableStepName);

        return Task.CompletedTask;
    }
}