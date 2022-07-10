using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;

namespace Stepping.TmProviders.Dtm;

public class DtmTmClient : ITmClient
{
    public virtual Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}