using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.Dtm.Grpc.Clients;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Secrets;
using Stepping.TmProviders.Dtm.Grpc.Steps;
using Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingDtmGrpcServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingDtmGrpc(this IServiceCollection services,
        Action<SteppingDtmGrpcOptions> setupAction)
    {
        services.Configure(setupAction);

        services.AddSteppingDtmGrpcServices();

        return services;
    }

    private static IServiceCollection AddSteppingDtmGrpcServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IActionApiTokenChecker, DefaultActionApiTokenChecker>();

        services.AddTransient<IStepToDtmStepConverter, ExecutableStepToDtmStepConverter>();
        services.TryAddTransient<ExecutableStepToDtmStepConverter>();

        services.AddTransient<IStepToDtmStepConverter, HttpRequestStepToDtmStepConverter>();
        services.TryAddTransient<HttpRequestStepToDtmStepConverter>();

        services.TryAddTransient<IStepToDtmStepConvertResolver, StepToDtmStepConvertResolver>();
        services.TryAddTransient<StepToDtmStepConvertResolver>();

        services.TryAddTransient<ITmClient, DtmGrpcTmClient>();
        services.TryAddTransient<DtmGrpcTmClient>();

        services.AddGrpcClient<Dtm.DtmClient>((serviceProvider, options) =>
        {
            var dtmGrpcOptions = serviceProvider.GetRequiredService<IOptions<SteppingDtmGrpcOptions>>().Value;
            options.Address = new Uri(dtmGrpcOptions.DtmGrpcUrl);
        });

        return services;
    }
}