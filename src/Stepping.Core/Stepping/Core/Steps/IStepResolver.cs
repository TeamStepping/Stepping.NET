namespace Stepping.Core.Steps;

public interface IStepResolver
{
    Task<IStep> ResolveAsync(string stepName);
}