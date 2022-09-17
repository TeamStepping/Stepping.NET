namespace Stepping.Core.Steps;

public class StepExecutionContext
{
    public string Gid { get; set; }

    public IServiceProvider ServiceProvider { get; }

    public CancellationToken CancellationToken { get; }

    public StepExecutionContext(string gid, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Gid = gid;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
    }
}