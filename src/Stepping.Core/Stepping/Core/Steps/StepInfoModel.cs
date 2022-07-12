namespace Stepping.Core.Steps;

public class StepInfoModel
{
    public string StepName { get; set; } = null!;

    public object? Args { get; set; }

    public StepInfoModel()
    {
    }

    public StepInfoModel(string stepName, object? args)
    {
        StepName = stepName;
        Args = args;
    }
}