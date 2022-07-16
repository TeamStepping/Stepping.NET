using System.Reflection;
using Stepping.Core.Steps;

namespace Stepping.Core.Options;

public class SteppingOptions
{
    /// <summary>
    /// The barrier table name. It will use the default value if you keep it <c>null</c>:<br /><br />
    /// SQL Server -> stepping.Barrier<br />
    /// SQLite -> stepping_barrier<br />
    /// MySQL -> stepping_barrier<br />
    /// PostgreSQL -> stepping.barrier<br />
    /// MongoDB -> stepping_barrier
    /// </summary>
    public string? BarrierTableName { get; set; } = null;

    public HashSet<Type> StepTypes { get; } = new();

    public void RegisterSteps(params Assembly[] assemblies)
    {
        var stepTypes = assemblies.Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => typeof(IStep).IsAssignableFrom(type));

        RegisterSteps(stepTypes.ToArray());
    }

    public void RegisterSteps(params Type[] stepTypes)
    {
        foreach (var stepType in stepTypes)
        {
            StepTypes.Add(stepType);
        }
    }
}