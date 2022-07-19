namespace Stepping.Core.Databases;

public interface IConnectionStringHasher
{
    Task<string> HashAsync(string connectionString);
}