namespace Stepping.Core.Infrastructures;

public interface ISteppingTenantIdProvider
{
    Task<string?> GetCurrentAsync();
}