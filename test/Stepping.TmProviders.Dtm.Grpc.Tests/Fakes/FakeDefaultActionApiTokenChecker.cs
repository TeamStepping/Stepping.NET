using Microsoft.Extensions.Options;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Secrets;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Fakes;

public class FakeDefaultActionApiTokenChecker : DefaultActionApiTokenChecker
{
    public static SteppingDtmGrpcOptions FakeOptions { get; } = new();

    public FakeDefaultActionApiTokenChecker(IOptions<SteppingDtmGrpcOptions> options) : base(options)
    {
        Options = FakeOptions;
    }
}