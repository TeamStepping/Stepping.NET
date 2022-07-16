using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(FakeWithArgsExecutableStepName)]
public class FakeWithArgsExecutableStep : ExecutableStep<string>
{
    public const string FakeWithArgsExecutableStepName = "FakeWithArgs";

    public FakeWithArgsExecutableStep(string args) : base(args)
    {
    }

    public override Task ExecuteAsync(StepExecutionContext context)
    {
        context.ServiceProvider.GetRequiredService<FakeService>();

        return Task.CompletedTask;
    }
}