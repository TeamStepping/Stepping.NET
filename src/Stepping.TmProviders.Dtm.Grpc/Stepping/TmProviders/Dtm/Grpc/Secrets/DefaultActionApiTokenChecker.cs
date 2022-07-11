using Microsoft.Extensions.Options;
using Stepping.TmProviders.Dtm.Grpc.Options;

namespace Stepping.TmProviders.Dtm.Grpc.Secrets;

public class DefaultActionApiTokenChecker : IActionApiTokenChecker
{
    private readonly SteppingDtmGrpcOptions _options;

    public DefaultActionApiTokenChecker(IOptions<SteppingDtmGrpcOptions> options)
    {
        _options = options.Value;
    }
    
    public virtual Task<bool> IsCorrectAsync(string token)
    {
        return Task.FromResult(token.Equals(_options.ActionApiToken));
    }
}