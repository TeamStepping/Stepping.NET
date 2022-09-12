using Xunit;

namespace Stepping.TmProviders.LocalTm.MongoDb.Tests;

[CollectionDefinition(Name)]
public class MongoTestCollection : ICollectionFixture<MongoDbFixture>
{
    public const string Name = "MongoDB LocalTm Collection";
}
