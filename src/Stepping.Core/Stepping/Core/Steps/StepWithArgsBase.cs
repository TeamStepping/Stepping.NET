namespace Stepping.Core.Steps;

public class StepWithArgsBase<TArgs> : IStep<TArgs> where TArgs : class
{
    public virtual TArgs Args { get; }

    public virtual object GetArgs() => Args;

    public StepWithArgsBase(TArgs args)
    {
        Args = args;
    }
}