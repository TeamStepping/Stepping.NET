using Stepping.Core.Databases;
using Stepping.TmProviders.LocalTm.Steps;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public interface ILocalTmManager
{
    Task PrepareAsync(string gid, LocalTmStepModel steps, SteppingDbContextLookupInfoModel steppingDbContextLookupInfo,
        CancellationToken cancellationToken = default);

    Task SubmitAsync(string gid, CancellationToken cancellationToken = default);

    Task ProcessPendingAsync(CancellationToken cancellationToken = default);

    Task ProcessSubmitAsync(string gid, CancellationToken cancellationToken = default);
}