using System.Collections.Concurrent;

namespace Stepping.Core.Steps;

public class StepNameProvider : IStepNameProvider
{
    protected ConcurrentDictionary<Type, string> CachedNames { get; } = new();

    public virtual string Get<TStep>() where TStep : IStep
    {
        var stepType = typeof(TStep);

        return Get(stepType);
    }

    public virtual string Get(Type stepType)
    {
        if (!CachedNames.ContainsKey(stepType))
        {
            CachedNames[stepType] = StepNameAttribute.GetStepName(stepType);
        }

        return CachedNames[stepType];
    }
}