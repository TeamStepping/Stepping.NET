using Microsoft.Extensions.Options;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Secrets;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Fakes;

public class FakeDefaultActionApiTokenChecker : DefaultActionApiTokenChecker
{
    public static SteppingDtmGrpcOptions FakeOptions { get; } = new()
    {
        ActionApiToken = "KLyqz0VS3mOc6VY1",
        AppGrpcUrl = "http://fakeurl.com:5000",
        DtmGrpcUrl = "http://fakeurl.com:36790"
    };

    public FakeDefaultActionApiTokenChecker(IOptions<SteppingDtmGrpcOptions> options) : base(options)
    {
        Options = FakeOptions;
    }
}