using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Exceptions;
using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

public class FakeControlExecutableStep : ExecutableStep
{
    public const string FakeControlExecutableStepStepName = "FakeControl";

    public static bool Throw { get; set; } = false;

    public override Task ExecuteAsync(StepExecutionContext context)
    {
        if (Throw)
        {
            throw new SteppingException("FakeControlExecutableStep stop.");
        }

        context.ServiceProvider.GetRequiredService<FakeService>();

        return Task.CompletedTask;
    }
}
