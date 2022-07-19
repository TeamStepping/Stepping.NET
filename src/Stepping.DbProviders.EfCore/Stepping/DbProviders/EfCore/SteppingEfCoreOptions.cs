namespace Stepping.DbProviders.EfCore;

public class SteppingEfCoreOptions
{
    /// <summary>
    /// It's used by <see cref="DefaultEfCoreSteppingDbContextProvider"/>.
    /// Keep it <c>null</c> if your DbContext has set a default connection string.
    /// You can also replace the default implementation to customize the connection string lookup.
    /// </summary>
    public string? DefaultConnectionString { get; set; }
}