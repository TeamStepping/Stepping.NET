namespace Stepping.TmProviders.LocalTm.MongoDb;

public class LocalTmMongoDbOptions
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;
}
