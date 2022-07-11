namespace Stepping.Core.Steps;

public class StepInfoModel
{
    public Type Type { get; set; } = null!;

    public object? Args { get; set; }

    public StepInfoModel()
    {
    }

    public StepInfoModel(Type type, object? args)
    {
        Type = type;
        Args = args;
    }
}