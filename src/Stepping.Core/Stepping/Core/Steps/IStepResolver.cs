namespace Stepping.Core.Steps;

public interface IStepResolver
{
    IStep Resolve(string stepName, object? args = null);

    IStep Resolve<TStep>(object? args = null) where TStep : IStep;
}