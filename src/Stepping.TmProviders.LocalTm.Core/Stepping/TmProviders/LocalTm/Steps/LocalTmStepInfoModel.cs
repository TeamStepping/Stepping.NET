namespace Stepping.TmProviders.LocalTm.Steps;

public class LocalTmStepInfoModel
{
    public string StepName { get; set; }

    public string? StepArgs { get; set; }

    public bool Executed { get; set; }

    public LocalTmStepInfoModel(string stepName, string? stepArgs)
    {
        StepName = stepName;
        StepArgs = stepArgs;
        Executed = false;
    }

    public void MarkAsExecuted()
    {
        Executed = true;
    }
}
