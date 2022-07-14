using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

public class FakeExecutableStep : ExecutableStep
{
    public const string FakeExecutableStepName = "Fake";

    public override string StepName => FakeExecutableStepName;

    public FakeExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        ServiceProvider.GetRequiredService<FakeService>();

        return Task.CompletedTask;
    }
}