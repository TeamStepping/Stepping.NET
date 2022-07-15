using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class StepExecutorTests : SteppingCoreTestBase
{
    public record InvalidArgsType;

    protected IStepExecutor StepExecutor { get; }
    protected IStepArgsSerializer StepArgsSerializer { get; }

    public StepExecutorTests()
    {
        StepExecutor = ServiceProvider.GetRequiredService<IStepExecutor>();
        StepArgsSerializer = ServiceProvider.GetRequiredService<IStepArgsSerializer>();
    }

    [Fact]
    public async Task Should_Execute_Step()
    {
        await Should.NotThrowAsync(() => StepExecutor.ExecuteAsync(FakeExecutableStep.FakeExecutableStepName, null));

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await StepExecutor.ExecuteAsync(FakeExecutableStep.FakeExecutableStepName,
                Encoding.UTF8.GetString(await StepArgsSerializer.SerializeAsync(new InvalidArgsType()))));
    }

    [Fact]
    public async Task Should_Execute_Step_With_Args()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            StepExecutor.ExecuteAsync(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName, null));

        await Should.NotThrowAsync(async () =>
            await StepExecutor.ExecuteAsync(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName,
                Encoding.UTF8.GetString(await StepArgsSerializer.SerializeAsync("my-input"))));
    }
}