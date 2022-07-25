using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Options;

namespace Stepping.TmProviders.LocalTm.Store;

public interface ILocalTmStore
{
    Task<TmTransactionModel> GetAsync(string gid, CancellationToken cancellationToken = default);

    Task CreateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default);

    Task UpdateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Pending TmTransaction List (read-only)
    /// <para>
    /// Condition (status != '<see cref="LocalTmConst.StatusFinish"/>' && status != '<see cref="LocalTmConst.StatusRollback"/>' && (NextRetryTime <= UTCNOW() || NextRetryTime == null) && CreateTime >= UTCNOW().Add(-<see cref="LocalTmOptions.Timeout"/>))
    /// </para>
    /// <para>
    /// Order By NextRetryTime
    /// </para>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TmTransactionModel>> GetPendingListAsync(CancellationToken cancellationToken = default);
}
