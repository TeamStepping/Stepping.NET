namespace Stepping.Core.Steps;

public abstract class ExecutableStep<TArgs> : StepWithArgsBase<TArgs>, IExecutableStep where TArgs : class
{
    protected ExecutableStep(TArgs args) : base(args)
    {
    }

    public abstract Task ExecuteAsync(StepExecutionContext context);
}

public abstract class ExecutableStep : IExecutableStep, IStepWithoutArgs
{
    public abstract Task ExecuteAsync(StepExecutionContext context);
}