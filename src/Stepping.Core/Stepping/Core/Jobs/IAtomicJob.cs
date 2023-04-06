using Stepping.Core.Databases;
using Stepping.Core.Steps;
using Stepping.Core.TransactionManagers;

namespace Stepping.Core.Jobs;

public interface IAtomicJob
{
    string Gid { get; }

    List<IStep> Steps { get; }

    ITmJobConfigurations? TmOptions { get; set; }

    ISteppingDbContext? DbContext { get; }

    bool PrepareSent { get; }

    bool SubmitSent { get; }

    /// <summary>
    /// Add a step for the job to do in order.
    /// </summary>
    IAtomicJob AddStep<TStep>(TStep step) where TStep : IStep;

    /// <summary>
    /// Add a step for the job to do in order.
    /// </summary>
    IAtomicJob AddStep<TStep>() where TStep : IStepWithoutArgs;

    /// <summary>
    /// Send "prepare" to TM, insert a barrier record to DB, commit the DB transaction, and send "submit" to TM.
    /// Execute only the "submit" sending if the job is not with a DB transaction.
    /// The TM will execute the steps you added in order.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);
}