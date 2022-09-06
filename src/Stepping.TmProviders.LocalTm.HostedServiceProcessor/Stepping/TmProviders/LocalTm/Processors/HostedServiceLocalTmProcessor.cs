﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stepping.TmProviders.LocalTm.DistributedLocks;
using Stepping.TmProviders.LocalTm.TransactionManagers;

namespace Stepping.TmProviders.LocalTm.Processors;

public class HostedServiceLocalTmProcessor : IHostedService, ILocalTmProcessor
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            await ProcessAsync(cancellationToken);
        }
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}