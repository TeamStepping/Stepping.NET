namespace Stepping.Core.Infrastructures;

public class NullSteppingTenantIdProvider : ISteppingTenantIdProvider
{
    public virtual Task<string?> GetCurrentAsync()
    {
        return Task.FromResult<string?>(null);
    }
}