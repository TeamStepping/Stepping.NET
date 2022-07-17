namespace Stepping.Core.Extensions;

public static class SteppingTypeExtensions
{
    public static string GetTypeFullNameWithAssemblyName(this Type type)
    {
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }
}