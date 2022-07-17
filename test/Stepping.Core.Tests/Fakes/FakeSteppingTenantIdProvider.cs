using Stepping.Core.Infrastructures;

namespace Stepping.Core.Tests.Fakes;

public class FakeSteppingTenantIdProvider : ISteppingTenantIdProvider
{
    public static string MyTenantId { get; set; } = "my-tenant-id";

    public Task<string?> GetCurrentAsync()
    {
        return Task.FromResult<string?>(MyTenantId);
    }
}