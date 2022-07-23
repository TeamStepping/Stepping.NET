namespace Stepping.TmProviders.LocalTm.Steps;

public interface ILocalTmStepExecutor
{
    Task ExecuteAsync(string gid, LocalTmStepInfoModel stepInfoModel, CancellationToken cancellationToken = default);
}