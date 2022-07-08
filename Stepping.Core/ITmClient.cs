namespace Stepping.Core;

public interface ITmClient
{
    Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default);

    Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default);
}