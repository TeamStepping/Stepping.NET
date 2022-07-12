namespace Stepping.Core.Databases;

public interface ISteppingDbContext
{
    string DbProviderName { get; }

    string ConnectionString { get; }
}