namespace Stepping.Core.Options;

public class SteppingOptions
{
    /// <summary>
    /// The barrier table name. It will use the default value if you keep it <c>null</c>:<br /><br />
    /// SQL Server -> stepping.Barrier<br />
    /// MySQL -> stepping_barrier<br />
    /// PostgreSQL -> stepping.barrier<br />
    /// MongoDB -> stepping_barrier
    /// </summary>
    public string? BarrierTableName { get; set; } = null;
}