namespace Stepping.DbProviders.MongoDb.Tests;

public static class MongoDbTestConsts
{
    public static string Database { get; } = $"db_{Guid.NewGuid()}";
}