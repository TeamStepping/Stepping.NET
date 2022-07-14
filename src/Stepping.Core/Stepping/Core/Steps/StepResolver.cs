﻿using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Exceptions;

namespace Stepping.Core.Steps;

public class StepResolver : IStepResolver
{
    protected static Dictionary<string, Type>? CachedTypes { get; set; }

    private static readonly object SyncObj = new();

    protected IServiceProvider ServiceProvider { get; }

    public StepResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual async Task<IStep> ResolveAsync(string stepName) => (IStep)await InternalResolveAsync(stepName);

    protected virtual Task<object> InternalResolveAsync(string stepName)
    {
        if (CachedTypes is null)
        {
            lock (SyncObj)
            {
                CachedTypes ??= CreateCachedTypes();
            }
        }

        if (!CachedTypes.ContainsKey(stepName))
        {
            throw new SteppingException("Invalid StepName.");
        }

        return Task.FromResult(ServiceProvider.GetRequiredService(CachedTypes[stepName]));
    }

    protected virtual Dictionary<string, Type> CreateCachedTypes()
    {
        var stepNameProvider = ServiceProvider.GetRequiredService<IStepNameProvider>();
        var steps = ServiceProvider.GetRequiredService<IEnumerable<IStep>>();

        return steps.ToDictionary(step => stepNameProvider.Get(step.GetType()), step => step.GetType());
    }
}