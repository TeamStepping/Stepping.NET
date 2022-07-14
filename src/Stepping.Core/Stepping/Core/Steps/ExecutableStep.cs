namespace Stepping.Core.Steps;

public abstract class ExecutableStep<TArgs> : ExecutableStepBase, IStep<TArgs> where TArgs : class
{
    public ExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public abstract Task ExecuteAsync(TArgs args);

    public override Task ExecuteAsync() => throw new InvalidOperationException();

    public override Task ExecuteAsync(object args) => ExecuteAsync((TArgs)args);
}

public abstract class ExecutableStep : ExecutableStepBase, IStepWithoutArgs
{
    protected ExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public override Task ExecuteAsync(object args) => throw new InvalidOperationException();
}

public abstract class ExecutableStepBase : StepBase, IExecutableStep
{
    protected IServiceProvider ServiceProvider { get; }

    public ExecutableStepBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public abstract Task ExecuteAsync();

    public abstract Task ExecuteAsync(object args);
}