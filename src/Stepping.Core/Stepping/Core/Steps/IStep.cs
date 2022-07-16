namespace Stepping.Core.Steps;

public interface IStep
{
}

public interface IStep<out TArgs> : IStepWithArgs where TArgs : class
{
    TArgs Args { get; }
}