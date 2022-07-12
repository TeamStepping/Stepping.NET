using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Stepping.Core.Steps;

public class StepNameProvider : IStepNameProvider
{
    protected static ConcurrentDictionary<Type, string> CachedNames { get; set; } = new();

    protected IServiceProvider ServiceProvider { get; }

    public StepNameProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual Task<string> GetAsync<TStep>() where TStep : IStep
    {
        var type = typeof(TStep);

        if (CachedNames.ContainsKey(type))
        {
            return Task.FromResult(CachedNames[type]);
        }

        var step = ServiceProvider.GetRequiredService<TStep>();

        CachedNames[type] = step.StepName;

        return Task.FromResult(step.StepName);
    }
}