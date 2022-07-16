namespace Stepping.Core.Steps;

public class StepExecutionContext
{
    public string Gid { get; set; }

    public IServiceProvider ServiceProvider { get; }

    public StepExecutionContext(string gid, IServiceProvider serviceProvider)
    {
        Gid = gid;
        ServiceProvider = serviceProvider;
    }
}