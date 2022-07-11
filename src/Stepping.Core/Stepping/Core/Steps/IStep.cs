namespace Stepping.Core.Steps;

public interface IStep
{
    string StepName { get; }
}

public interface IStep<TArgs> : IStep where TArgs : class
{
}