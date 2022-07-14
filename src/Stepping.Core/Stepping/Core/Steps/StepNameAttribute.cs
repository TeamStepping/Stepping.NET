using System.Reflection;

namespace Stepping.Core.Steps;

public class StepNameAttribute : Attribute
{
    public string Name { get; }

    public StepNameAttribute(string name)
    {
        if (name is null or "")
        {
            throw new ArgumentException();
        }

        Name = name;
    }

    public virtual string GetName(Type type) => Name;

    public static string GetStepName<T>() => GetStepName(typeof(T));

    public static string GetStepName(Type type)
    {
        var nameAttribute = type.GetCustomAttribute<StepNameAttribute>();

        return nameAttribute == null ? type.FullName! : nameAttribute.GetName(type);
    }
}