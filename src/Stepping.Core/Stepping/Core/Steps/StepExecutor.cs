using Stepping.Core.Exceptions;

namespace Stepping.Core.Steps;

public class StepExecutor : IStepExecutor
{
    protected IStepResolver StepResolver { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected IStepArgsSerializer StepArgsSerializer { get; }

    public StepExecutor(
        IStepResolver stepResolver,
        IServiceProvider serviceProvider,
        IStepArgsSerializer stepArgsSerializer)
    {
        StepResolver = stepResolver;
        ServiceProvider = serviceProvider;
        StepArgsSerializer = stepArgsSerializer;
    }

    public virtual async Task ExecuteAsync(string gid, string executableStepName, string? argsToByteString,
        CancellationToken cancellationToken = default)
    {
        var step = StepResolver.Resolve(executableStepName, argsToByteString);

        var stepType = step.GetType();

        if (!typeof(IExecutableStep).IsAssignableFrom(stepType))
        {
            throw new SteppingException("Cannot execute a non-executable step.");
        }

        var executableStep = (IExecutableStep)step;

        if (argsToByteString is null || argsToByteString.Equals(string.Empty))
        {
            await executableStep.ExecuteAsync(new StepExecutionContext(gid, ServiceProvider, cancellationToken));
        }
        else
        {
            var argsType = GetArgsType(stepType);
            var args = await StepArgsSerializer.DeserializeAsync(argsToByteString, argsType);

            await executableStep.ExecuteAsync(new StepExecutionContext(gid, ServiceProvider, cancellationToken));
        }
    }

    protected virtual Type GetArgsType(Type stepType)
    {
        var baseType = stepType;

        while ((baseType = baseType.BaseType) is not null)
        {
            if (!baseType.IsGenericType)
            {
                continue;
            }

            var generic = baseType.GetGenericTypeDefinition();

            if (generic != typeof(ExecutableStep<>))
            {
                continue;
            }

            return baseType.GetGenericArguments()[0];
        }

        throw new InvalidOperationException();
    }
}