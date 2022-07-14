using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(FakeExecutableStepName)]
public class FakeExecutableStep : ExecutableStep
{
    public const string FakeExecutableStepName = "Fake";

    public FakeExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        ServiceProvider.GetRequiredService<FakeService>();

        return Task.CompletedTask;
    }
}