namespace Stepping.DbProviders.MongoDb;

public class SteppingMongoDbOptions
{
    /// <summary>
    /// It's used by <see cref="DefaultMongoDbSteppingDbContextProvider"/>.
    /// You can also replace the default implementation to customize the connection string lookup.
    /// </summary>
    public string DefaultConnectionString { get; set; } = null!;
}