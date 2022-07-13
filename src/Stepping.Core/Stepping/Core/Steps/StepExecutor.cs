namespace Stepping.Core.Steps;

public class StepExecutor : IStepExecutor
{
    protected IStepResolver StepResolver { get; }
    protected IStepArgsSerializer StepArgsSerializer { get; }

    public StepExecutor(
        IStepResolver stepResolver,
        IStepArgsSerializer stepArgsSerializer)
    {
        StepResolver = stepResolver;
        StepArgsSerializer = stepArgsSerializer;
    }

    public virtual async Task ExecuteAsync(string executableStepName, string? argsToByteString)
    {
        var step = await StepResolver.ResolveAsync(executableStepName);
        var stepType = step.GetType();

        if (typeof(ExecutableStep).IsAssignableFrom(stepType))
        {
            var executableStep = (ExecutableStep)step;

            if (argsToByteString is null || argsToByteString.Equals(string.Empty))
            {
                await executableStep.ExecuteAsync();
            }
            else
            {
                var argsType = GetArgsType(stepType);
                var args = await StepArgsSerializer.DeserializeAsync(argsToByteString, argsType);

                await executableStep.ExecuteAsync(args);
            }
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