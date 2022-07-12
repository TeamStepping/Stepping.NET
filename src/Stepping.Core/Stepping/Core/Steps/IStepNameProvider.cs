namespace Stepping.Core.Steps;

public interface IStepNameProvider
{
    Task<string> GetAsync<TStep>() where TStep : IStep;
}