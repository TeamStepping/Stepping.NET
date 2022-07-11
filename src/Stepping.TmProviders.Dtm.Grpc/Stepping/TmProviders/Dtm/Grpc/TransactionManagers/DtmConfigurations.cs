using Stepping.Core.TransactionManagers;

namespace Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

public class DtmJobConfigurations : ITmJobConfigurations
{
    public long TimeoutToFail { get; set; }

    public long RetryInterval { get; set; }

    public Dictionary<string, string> BranchHeaders { get; set; } = new();

    public HashSet<string> PassthroughHeaders { get; set; } = new();
}