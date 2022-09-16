namespace Stepping.TmProviders.LocalTm.MongoDb.Tests;

public static class MongoDbTestConsts
{
    public static string Database { get; } = $"db_{Guid.NewGuid()}";
}