namespace Stepping.Core.Steps;

public interface IExecutableStep : IStep
{
    Task ExecuteAsync(StepExecutionContext context);
}