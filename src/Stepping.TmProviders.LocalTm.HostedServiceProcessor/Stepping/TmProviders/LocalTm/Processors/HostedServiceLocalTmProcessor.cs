using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stepping.TmProviders.LocalTm.DistributedLocks;
using Stepping.TmProviders.LocalTm.TransactionManagers;

namespace Stepping.TmProviders.LocalTm.Processors;

public class HostedServiceLocalTmProcessor : BackgroundService, ILocalTmProcessor
{
    private readonly ILogger<HostedServiceLocalTmProcessor> _logger;
    private readonly PeriodicTimer _timer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public HostedServiceLocalTmProcessor(
        ILogger<HostedServiceLocalTmProcessor> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessAsync(stoppingToken);
        }
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var steppingDistributedLock = scope.ServiceProvider.GetRequiredService<ISteppingDistributedLock>();

        await using var handle = await steppingDistributedLock.TryAcquireAsync("LocalTmHostedService", cancellationToken: cancellationToken);

        if (handle == null)
        {
            _logger.LogInformation("Local transaction process pending try acquire lock failed.");
            return;
        }

        await scope.ServiceProvider
                .GetRequiredService<ILocalTmManager>()
                .ProcessPendingAsync(cancellationToken);
    }
}
