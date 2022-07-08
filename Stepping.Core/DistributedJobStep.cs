namespace Stepping.Core;

public class DistributedJobStep<TArgs> : IDistributedJobStep
{
    public Func<IServiceProvider, TArgs, Task> Action { get; }

    public TArgs Args { get; }

    public DistributedJobStep(Func<IServiceProvider, TArgs, Task> action, TArgs args)
    {
        Action = action;
        Args = args;
    }

    public virtual Task DoAsync(IServiceProvider serviceProvider)
    {
        return Action.Invoke(serviceProvider, Args);
    }
}