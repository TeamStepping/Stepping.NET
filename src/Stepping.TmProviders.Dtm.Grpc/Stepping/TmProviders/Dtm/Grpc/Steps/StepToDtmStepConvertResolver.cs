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

    public StepToDtmStepConvertResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual async Task<DtmStepInfoModel> ResolveAsync(string stepName, object? args)
    {
        if (CachedConverters is null)
        {
            lock (SyncObj)
            {
                CachedConverters ??=
                    ServiceProvider.GetRequiredService<IEnumerable<IStepToDtmStepConverter>>().ToList();
            }
        }

        if (CachedMappings.ContainsKey(stepName))
        {
            return await CachedMappings[stepName].ConvertAsync(stepName, args);
        }

        foreach (var converter in CachedConverters)
        {
            var stepResolver = ServiceProvider.GetRequiredService<IStepResolver>();
            var step = await stepResolver.ResolveAsync(stepName);

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