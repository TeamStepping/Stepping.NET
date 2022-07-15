using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(FakeExecutableStepName)]
public class FakeExecutableStep : ExecutableStep
{
    public const string FakeExecutableStepName = "Fake";

    public override Task ExecuteAsync(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<FakeService>();

        return Task.CompletedTask;
    }
}