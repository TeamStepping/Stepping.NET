using Stepping.Core.TransactionManagers;

namespace Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

public class DtmJobConfigurations : ITmJobConfigurations
{
    public bool WaitResult { get; set; }

    public long TimeoutToFail { get; set; }

    public long RetryInterval { get; set; }

    public Dictionary<string, string> BranchHeaders { get; set; } = new();

    public long RetryLimit { get; set; }
}