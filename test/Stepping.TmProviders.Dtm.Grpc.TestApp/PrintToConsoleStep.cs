using Stepping.Core.Steps;

namespace Stepping.TmProviders.Dtm.Grpc.TestApp;

public class PrintToConsoleStep : ExecutableStep
{
    public override Task ExecuteAsync(StepExecutionContext context)
    {
        Console.WriteLine("Step 0 `PrintToConsoleStep` executed.");

        return Task.CompletedTask;
    }
}