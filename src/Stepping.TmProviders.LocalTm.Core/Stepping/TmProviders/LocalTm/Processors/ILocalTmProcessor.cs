namespace Stepping.TmProviders.LocalTm.Processors;

public interface ILocalTmProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}
