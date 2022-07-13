namespace Stepping.Core.Steps;

public interface IExecutableStep : IStep
{
    Task ExecuteAsync();

    Task ExecuteAsync(object args);
}