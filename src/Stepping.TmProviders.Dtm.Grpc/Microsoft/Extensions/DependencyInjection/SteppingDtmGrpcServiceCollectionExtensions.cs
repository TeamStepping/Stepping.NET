using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.TransactionManagers;
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
        services.AddSteppingDtmGrpcServices();

        services.Configure(setupAction);

        return services;
    }

    private static IServiceCollection AddSteppingDtmGrpcServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IActionApiTokenChecker, DefaultActionApiTokenChecker>();

        services.TryAddTransient<IStepToDtmStepConverter, ExecutableStepToDtmStepConverter>();
        services.TryAddTransient<ExecutableStepToDtmStepConverter>();

        services.TryAddTransient<IStepToDtmStepConverter, HttpRequestStepToDtmStepConverter>();
        services.TryAddTransient<HttpRequestStepToDtmStepConverter>();

        services.TryAddTransient<IStepToDtmStepConvertResolver, StepToDtmStepConvertResolver>();
        services.TryAddTransient<StepToDtmStepConvertResolver>();

        services.TryAddTransient<ITmClient, DtmGrpcTmClient>();
        services.TryAddTransient<DtmGrpcTmClient>();

        return services;
    }
}