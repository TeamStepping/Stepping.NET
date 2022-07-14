namespace Stepping.Core.Steps;

public interface IStep
{
}

public interface IStep<TArgs> : IStep where TArgs : class
{
}