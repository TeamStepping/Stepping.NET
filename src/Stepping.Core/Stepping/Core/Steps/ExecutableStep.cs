namespace Stepping.Core.Steps;

public abstract class ExecutableStep<TArgs> : ExecutableStep, IStep<TArgs> where TArgs : class
{
    public ExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public abstract Task ExecuteAsync(TArgs args);

    public override Task ExecuteAsync() => throw new InvalidOperationException();

    public override Task ExecuteAsync(object args) => ExecuteAsync((TArgs)args);
}

public abstract class ExecutableStep : StepBase
{
    protected IServiceProvider ServiceProvider { get; }

    public ExecutableStep(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public abstract Task ExecuteAsync();

    public virtual Task ExecuteAsync(object args) => throw new InvalidOperationException();
}