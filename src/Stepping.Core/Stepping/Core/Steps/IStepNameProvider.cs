namespace Stepping.Core.Steps;

public interface IStepNameProvider
{
    string Get<TStep>() where TStep : IStep;

    string Get(Type stepType);
}