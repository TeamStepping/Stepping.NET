namespace Stepping.Core.Steps;

public class StepBase : IStep
{
    public virtual string StepName { get; }

    public StepBase()
    {
        var type = GetType();
        StepName = $"{type.FullName}, {type.Assembly.GetName().Name}";
    }
}