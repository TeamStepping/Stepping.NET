namespace Stepping.Core.Steps;

public interface IStepExecutor
{
    Task ExecuteAsync(string gid, string executableStepName, string? argsToByteString);
}