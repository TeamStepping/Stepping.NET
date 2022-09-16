using Microsoft.Extensions.Options;
using Stepping.TmProviders.Dtm.Grpc.Options;

namespace Stepping.TmProviders.Dtm.Grpc.Secrets;

public class DefaultActionApiTokenChecker : IActionApiTokenChecker
{
    protected SteppingDtmGrpcOptions Options { get; set; }

    public DefaultActionApiTokenChecker(IOptions<SteppingDtmGrpcOptions> options)
    {
        Options = options.Value;
    }

    public virtual Task<bool> IsCorrectAsync(string? token)
    {
        if (Options.ActionApiToken is null || Options.ActionApiToken.Equals(string.Empty))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(token == Options.ActionApiToken);
    }
}