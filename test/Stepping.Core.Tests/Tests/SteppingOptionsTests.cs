using Shouldly;
using Stepping.Core.Options;
using Stepping.TestBase;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class SteppingOptionsTests : SteppingCoreTestBase
{
    [Fact]
    public Task Should_Register_Steps_By_Types()
    {
        var options = new SteppingOptions();

        options.StepTypes.ShouldBeEmpty();

        options.RegisterSteps(typeof(FakeExecutableStep), typeof(FakeWithArgsExecutableStep));

        options.StepTypes.Count.ShouldBe(2);
        options.StepTypes.ShouldContain(typeof(FakeExecutableStep));
        options.StepTypes.ShouldContain(typeof(FakeWithArgsExecutableStep));

        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Register_Steps_By_Assembly()
    {
        var options = new SteppingOptions();

        options.StepTypes.ShouldBeEmpty();

        options.RegisterSteps(typeof(SteppingTestBase).Assembly);

        options.StepTypes.Count.ShouldBe(2);
        options.StepTypes.ShouldContain(typeof(FakeExecutableStep));
        options.StepTypes.ShouldContain(typeof(FakeWithArgsExecutableStep));

        return Task.CompletedTask;
    }
}