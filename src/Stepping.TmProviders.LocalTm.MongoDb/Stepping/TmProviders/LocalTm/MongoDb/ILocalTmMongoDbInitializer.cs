using Stepping.Core.Databases;

namespace Stepping.TmProviders.LocalTm.MongoDb;

public interface ILocalTmMongoDbInitializer
{
    Task TryInitializeAsync();
}