using Xunit;

namespace Stepping.DbProviders.MongoDb.Tests;

[CollectionDefinition(Name)]
public class MongoTestCollection : ICollectionFixture<MongoDbFixture>
{
    public const string Name = "MongoDB Collection";
}
