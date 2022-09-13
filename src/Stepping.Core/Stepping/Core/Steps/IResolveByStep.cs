namespace Stepping.Core.Steps;

/// <summary>
/// Specify type for the <see cref="IStepResolver"/> to create a step object.
/// </summary>
public interface IResolveByStep<TStep> where TStep : IStep
{
}