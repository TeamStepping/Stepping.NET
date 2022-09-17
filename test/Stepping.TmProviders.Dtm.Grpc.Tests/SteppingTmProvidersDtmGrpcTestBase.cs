using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.TransactionManagers;
using Stepping.TestBase;
using Stepping.TmProviders.Dtm.Grpc.Secrets;
using Stepping.TmProviders.Dtm.Grpc.Tests.Fakes;

namespace Stepping.TmProviders.Dtm.Grpc.Tests;

public abstract class SteppingTmProvidersDtmGrpcTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSteppingDtmGrpc(options =>
        {
            options.ActionApiToken = "KLyqz0VS3mOc6VY1";
            options.AppGrpcUrl = "http://fakeurl.com:5000";
            options.DtmGrpcUrl = "http://fakeurl.com:36790";
        });

        base.ConfigureServices(services);

        services.AddSingleton<IActionApiTokenChecker, FakeDefaultActionApiTokenChecker>();

        services.AddTransient<ITmClient, FakeDtmGrpcTmClient>();
        services.AddTransient<FakeDtmGrpcTmClient>();
    }
}