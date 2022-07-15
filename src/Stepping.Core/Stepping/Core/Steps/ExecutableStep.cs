namespace Stepping.Core.Steps;

public abstract class ExecutableStep<TArgs> : IExecutableStep, IStepWithArgs, IStep<TArgs> where TArgs : class
{
    public TArgs Args { get; }

    protected ExecutableStep(TArgs args)
    {
        Args = args;
    }

    public abstract Task ExecuteAsync(StepExecutionContext context);

    public virtual object GetArgs() => Args;
}

public abstract class ExecutableStep : IExecutableStep, IStepWithoutArgs
{
    public abstract Task ExecuteAsync(StepExecutionContext context);
}