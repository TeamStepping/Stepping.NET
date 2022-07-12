using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.Core.Tests.Fakes;
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
    public async Task Should_Get_Step_Name()
    {
        (await StepNameProvider.GetAsync<FakeExecutableStep>()).ShouldBe(FakeExecutableStep.FakeExecutableStepName);
    }
}