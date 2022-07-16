namespace Stepping.Core.Steps;

public record StepResolverCachedTypeModel(Type Type, bool HasArgs)
{
    public Type Type { get; set; } = Type;

    public bool HasArgs { get; set; } = HasArgs;
}