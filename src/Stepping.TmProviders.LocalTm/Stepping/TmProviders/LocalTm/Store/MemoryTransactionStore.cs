using System.Collections.Concurrent;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Options;

namespace Stepping.TmProviders.LocalTm.Store;

public class MemoryTransactionStore : ITransactionStore
{
    private static readonly ConcurrentDictionary<string, TmTransactionModel> _memoryStore = new();

    protected ILogger<MemoryTransactionStore> Logger { get; }

    protected IOptionsMonitor<LocalTmOptions> OptionsMonitor { get; }

    protected ISteppingJsonSerializer SteppingJsonSerializer { get; }

    public MemoryTransactionStore(
        ILogger<MemoryTransactionStore> logger,
        IOptionsMonitor<LocalTmOptions> optionsMonitor,
        ISteppingJsonSerializer steppingJsonSerializer)
    {
        Logger = logger;
        OptionsMonitor = optionsMonitor;
        SteppingJsonSerializer = steppingJsonSerializer;
    }

    public virtual async Task CreateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("You are using the ITransactionStore's memory implementation, please do not use it in production environment!");

        tmTransaction.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        var cloneTmTransaction = await DeepCloneAsync(tmTransaction);

        if (!_memoryStore.TryAdd(tmTransaction.Gid, cloneTmTransaction))
        {
            throw new SteppingException($"Local transaction '{tmTransaction.Gid}' exists.");
        }
    }

    public virtual async Task<TmTransactionModel> GetAsync(string gid, CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("You are using the ITransactionStore's memory implementation, please do not use it in production environment!");

        if (!_memoryStore.TryGetValue(gid, out var tmTransaction))
        {
            throw new SteppingException($"Local transaction '{gid}' not exists.");
        }

        return await DeepCloneAsync(tmTransaction);
    }

    public virtual async Task UpdateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("You are using the ITransactionStore's memory implementation, please do not use it in production environment!");

        var cloneTmTransaction = await DeepCloneAsync(tmTransaction);

        _memoryStore.AddOrUpdate(
            tmTransaction.Gid,
            _ => throw new SteppingException($"Local transaction '{tmTransaction.Gid}' update failed."),
            (_, existTmTransaction) =>
            {
                if (tmTransaction.ConcurrencyStamp != existTmTransaction.ConcurrencyStamp)
                {
                    throw new SteppingException($"Local transaction '{tmTransaction.Gid}' update failed.");
                }

                cloneTmTransaction.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                tmTransaction.ConcurrencyStamp = cloneTmTransaction.ConcurrencyStamp;
                return cloneTmTransaction;
            }
        );
    }

    public virtual async Task<List<TmTransactionModel>> GetPendingListAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("You are using the ITransactionStore's memory implementation, please do not use it in production environment!");

        var timeout = OptionsMonitor.CurrentValue.Timeout;

        var query = _memoryStore
            .Where(x =>
                x.Value.Status != LocalTmConst.StatusFinish && x.Value.Status != LocalTmConst.StatusRollback &&
                (x.Value.NextRetryTime == null || x.Value.NextRetryTime >= DateTime.UtcNow.Add(-timeout))
            )
            .OrderBy(x => x.Value.NextRetryTime)
            .Select(x => x.Value);

        var list = new List<TmTransactionModel>();
        foreach (var tmTransaction in query)
        {
            list.Add(await DeepCloneAsync(tmTransaction));
        }

        return list;
    }

    protected virtual Task<TmTransactionModel> DeepCloneAsync(TmTransactionModel tmTransactionModel)
    {
        var cloneTmTransaction = SteppingJsonSerializer.Deserialize<TmTransactionModel>(SteppingJsonSerializer.Serialize(tmTransactionModel));
        return Task.FromResult(cloneTmTransaction);
    }
}
