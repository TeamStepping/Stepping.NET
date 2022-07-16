using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stepping.Core.Exceptions;
using Stepping.Core.Options;

namespace Stepping.Core.Steps;

public class StepResolver : IStepResolver
{
    protected static Dictionary<string, StepResolverCachedTypeModel>? CachedTypes { get; set; }

    private static readonly object SyncObj = new();

    protected IServiceProvider ServiceProvider { get; }
    protected IStepNameProvider StepNameProvider { get; }

    public StepResolver(
        IServiceProvider serviceProvider,
        IStepNameProvider stepNameProvider)
    {
        ServiceProvider = serviceProvider;
        StepNameProvider = stepNameProvider;
    }

    public virtual IStep Resolve(string stepName, object? args = null) => (IStep)InternalResolve(stepName, args);

    public virtual IStep Resolve<TStep>(object? args = null) where TStep : IStep =>
        (IStep)InternalResolve(StepNameProvider.Get<TStep>(), args);

    protected virtual object InternalResolve(string stepName, object? args = null)
    {
        TryWarmUp(ServiceProvider);

        if (!CachedTypes!.ContainsKey(stepName))
        {
            throw new SteppingException("Invalid StepName.");
        }

        return CachedTypes[stepName].HasArgs
            ? ActivatorUtilities.CreateInstance(ServiceProvider, CachedTypes[stepName].Type, args!)
            : ActivatorUtilities.CreateInstance(ServiceProvider, CachedTypes[stepName].Type);
    }

    protected static Dictionary<string, StepResolverCachedTypeModel> CreateCachedTypes(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<SteppingOptions>>();
        var stepNameProvider = serviceProvider.GetRequiredService<IStepNameProvider>();

        return options.Value.StepTypes
            .ToDictionary(
                stepNameProvider.Get,
                type => new StepResolverCachedTypeModel(type, typeof(IStepWithArgs).IsAssignableFrom(type)));
    }

    public static void TryWarmUp(IServiceProvider serviceProvider)
    {
        if (CachedTypes is not null)
        {
            return;
        }

        lock (SyncObj)
        {
            CachedTypes ??= CreateCachedTypes(serviceProvider);
        }
    }
}