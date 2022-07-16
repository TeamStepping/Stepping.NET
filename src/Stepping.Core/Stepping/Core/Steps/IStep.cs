namespace Stepping.Core.Steps;

public interface IStep
{
}

public interface IStep<out TArgs> : IStep where TArgs : class
{
    TArgs Args { get; }
}