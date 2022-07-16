using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Exceptions;
using Stepping.Core.Steps;

namespace Stepping.TmProviders.Dtm.Grpc.Steps;

public class StepToDtmStepConvertResolver : IStepToDtmStepConvertResolver
{
    protected static List<IStepToDtmStepConverter>? CachedConverters { get; set; }
    protected static ConcurrentDictionary<string, IStepToDtmStepConverter> CachedMappings { get; set; } = new();

    private static readonly object SyncObj = new();

    protected IServiceProvider ServiceProvider { get; }
    protected IStepNameProvider StepNameProvider { get; }

    public StepToDtmStepConvertResolver(
        IServiceProvider serviceProvider,
        IStepNameProvider stepNameProvider)
    {
        ServiceProvider = serviceProvider;
        StepNameProvider = stepNameProvider;
    }

    public virtual async Task<DtmStepInfoModel> ResolveAsync(IStep step)
    {
        if (CachedConverters is null)
        {
            lock (SyncObj)
            {
                CachedConverters ??=
                    ServiceProvider.GetRequiredService<IEnumerable<IStepToDtmStepConverter>>().ToList();
            }
        }

        var stepName = StepNameProvider.Get(step.GetType());
        var args = step is IStepWithArgs withArgs ? withArgs.GetArgs() : null;

        if (CachedMappings.ContainsKey(stepName))
        {
            return await CachedMappings[stepName].ConvertAsync(stepName, args);
        }

        foreach (var converter in CachedConverters)
        {
            if (!await converter.CanConvertAsync(step))
            {
                continue;
            }

            CachedMappings[stepName] = converter;

            return await converter.ConvertAsync(stepName, args);
        }

        throw new SteppingException($"Step converter for {stepName} not found.");
    }
}