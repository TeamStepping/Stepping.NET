namespace Stepping.Core.Databases;

public interface ISteppingDbContextProviderResolver
{
    Task<ISteppingDbContextProvider> ResolveAsync(string dbProviderName);
}