using Stepping.Core.Exceptions;

namespace Stepping.Core.Steps;

public class StepExecutor : IStepExecutor
{
    protected IStepResolver StepResolver { get; }
    protected IServiceProvider ServiceProvider { get; }

    public StepExecutor(
        IStepResolver stepResolver,
        IServiceProvider serviceProvider)
    {
        StepResolver = stepResolver;
        ServiceProvider = serviceProvider;
    }

    public virtual async Task ExecuteAsync(string gid, string executableStepName, string? argsToByteString,
        CancellationToken cancellationToken = default)
    {
        var args = await StepResolver.ResolveArgsAsync(executableStepName, argsToByteString);
        var step = StepResolver.Resolve(executableStepName, args);

        var stepType = step.GetType();

        if (!typeof(IExecutableStep).IsAssignableFrom(stepType))
        {
            throw new SteppingException("Cannot execute a non-executable step.");
        }

        var executableStep = (IExecutableStep)step;
        await executableStep.ExecuteAsync(new StepExecutionContext(gid, ServiceProvider, cancellationToken));
    }
}