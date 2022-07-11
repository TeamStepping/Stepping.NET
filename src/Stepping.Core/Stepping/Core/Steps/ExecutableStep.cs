namespace Stepping.Core.Steps;

public abstract class ExecutableStep<TArgs> : ExecutableStep, IStep<TArgs> where TArgs : class
{
    protected IServiceProvider ServiceProvider { get; }

    public ExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public abstract Task ExecuteAsync(TArgs args);

    public override Task ExecuteAsync(object args) => ExecuteAsync((TArgs)args);
}

public abstract class ExecutableStep : StepBase
{
    public ExecutableStep(IServiceProvider serviceProvider)
    {
    }

    public abstract Task ExecuteAsync();

    public virtual Task ExecuteAsync(object args) => ExecuteAsync();
}