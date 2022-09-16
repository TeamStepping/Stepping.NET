namespace Stepping.TmProviders.LocalTm.Steps;

public class LocalTmStepInfoModel
{
    public string StepName { get; set; }

    public string? ArgsToByteString { get; set; }

    public bool Executed { get; set; }

    public LocalTmStepInfoModel(string stepName, string? argsToByteString)
    {
        StepName = stepName;
        ArgsToByteString = argsToByteString;
        Executed = false;
    }

    public void MarkAsExecuted()
    {
        Executed = true;
    }
}
