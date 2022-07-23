using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stepping.TmProviders.LocalTm.TransactionManagers;

namespace Stepping.TmProviders.LocalTm.HostedService;

public class LocalTmHostedService : IHostedService
{
    private readonly ILogger<LocalTmHostedService> _logger;
    private readonly PeriodicTimer _timer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LocalTmHostedService(
        ILogger<LocalTmHostedService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        while (await _timer.WaitForNextTickAsync())
        {
            await DoWorkAsync(cancellationToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await scope.ServiceProvider
            .GetRequiredService<ILocalTmProcessor>()
            .ProcessPendingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}
