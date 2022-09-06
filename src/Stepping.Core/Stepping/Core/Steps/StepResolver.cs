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
    protected IStepArgsSerializer StepArgsSerializer { get; }

    public StepResolver(
        IServiceProvider serviceProvider,
        IStepNameProvider stepNameProvider,
        IStepArgsSerializer stepArgsSerializer)
    {
        ServiceProvider = serviceProvider;
        StepNameProvider = stepNameProvider;
        StepArgsSerializer = stepArgsSerializer;
    }

    public virtual IStep Resolve(string stepName, object? args = null) => (IStep)InternalResolve(stepName, args);

    public virtual IStep Resolve<TStep>(object? args = null) where TStep : IStep =>
        (IStep)InternalResolve(StepNameProvider.Get<TStep>(), args);

    public virtual async Task<object?> ResolveArgsAsync(string stepName, string? argsToByteString = null)
    {
        if (argsToByteString is null or "")
        {
            return null;
        }

        var stepTypeModel = GetStepTypeModel(stepName);
        var argsType = GetArgsType(stepTypeModel.Type);

        return await StepArgsSerializer.DeserializeAsync(argsToByteString, argsType);
    }

    protected virtual object InternalResolve(string stepName, object? args = null)
    {
        var stepTypeModel = GetStepTypeModel(stepName);

        return stepTypeModel.HasArgs
            ? ActivatorUtilities.CreateInstance(ServiceProvider, stepTypeModel.Type, args!)
            : ActivatorUtilities.CreateInstance(ServiceProvider, stepTypeModel.Type);
    }

    protected virtual StepResolverCachedTypeModel GetStepTypeModel(string stepName)
    {
        TryWarmUp(ServiceProvider);

        if (!CachedTypes!.ContainsKey(stepName))
        {
            throw new SteppingException("Invalid StepName.");
        }

        return CachedTypes[stepName];
    }

    protected virtual Type GetArgsType(Type stepType)
    {
        var baseType = stepType;

        while ((baseType = baseType.BaseType) is not null)
        {
            if (!baseType.IsGenericType)
            {
                continue;
            }

            var generic = baseType.GetGenericTypeDefinition();

            if (generic != typeof(ExecutableStep<>))
            {
                continue;
            }

            return baseType.GetGenericArguments()[0];
        }

        throw new InvalidOperationException();
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