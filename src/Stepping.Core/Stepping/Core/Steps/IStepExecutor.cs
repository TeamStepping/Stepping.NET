namespace Stepping.Core.Steps;

public interface IStepExecutor
{
    Task ExecuteAsync(string executableStepName, string? argsToByteString);
}