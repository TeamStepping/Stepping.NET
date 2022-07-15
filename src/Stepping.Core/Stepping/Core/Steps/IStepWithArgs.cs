namespace Stepping.Core.Steps;

public interface IStepWithArgs : IStep
{
    object GetArgs();
}
