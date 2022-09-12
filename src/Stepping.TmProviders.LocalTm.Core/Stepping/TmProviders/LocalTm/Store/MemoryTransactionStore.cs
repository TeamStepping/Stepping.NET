using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Options;
using Stepping.TmProviders.LocalTm.Timing;

namespace Stepping.TmProviders.LocalTm.Store;

public class MemoryTransactionStore : ITransactionStore
{
    private static readonly ConcurrentDictionary<string, TmTransactionModel> _memoryStore = new();

    protected ILogger<MemoryTransactionStore> Logger { get; }

    protected LocalTmOptions Options { get; }

    protected ISteppingJsonSerializer SteppingJsonSerializer { get; }

    protected ISteppingClock SteppingClock { get; }

    public MemoryTransactionStore(
        ILogger<MemoryTransactionStore> logger,
        IOptionsMonitor<LocalTmOptions> optionsMonitor,
        ISteppingJsonSerializer steppingJsonSerializer,
        ISteppingClock steppingClock)
    {
        Logger = logger;
        Options = optionsMonitor.CurrentValue;
        SteppingJsonSerializer = steppingJsonSerializer;
        SteppingClock = steppingClock;
    }

    public virtual async Task<List<TmTransactionModel>> GetPendingListAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("You are using the ITransactionStore's memory implementation, please do not use it in production environment!");

        var timeoutTime = SteppingClock.Now.Add(-Options.Timeout);

        var query = _memoryStore
            .Where(x =>
                x.Value.Status != LocalTmConst.StatusFinish && x.Value.Status != LocalTmConst.StatusRollback &&
                (x.Value.NextRetryTime == null || x.Value.NextRetryTime <= timeoutTime) &&
                x.Value.CreationTime <= timeoutTime
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

    public virtual async Task<TmTransactionModel> GetAsync(string gid, CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("You are using the ITransactionStore's memory implementation, please do not use it in production environment!");

        if (!_memoryStore.TryGetValue(gid, out var tmTransaction))
        {
            throw new SteppingException($"Local transaction '{gid}' not exists.");
        }

        return await DeepCloneAsync(tmTransaction);
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

    protected virtual Task<TmTransactionModel> DeepCloneAsync(TmTransactionModel tmTransactionModel)
    {
        var cloneTmTransaction = SteppingJsonSerializer.Deserialize<TmTransactionModel>(SteppingJsonSerializer.Serialize(tmTransactionModel));
        return Task.FromResult(cloneTmTransaction);
    }
}
