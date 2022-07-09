using Stepping.Core;

namespace TmProviders.Dtm;

public class DtmTmClient : ITmClient
{
    public virtual async Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual async Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}