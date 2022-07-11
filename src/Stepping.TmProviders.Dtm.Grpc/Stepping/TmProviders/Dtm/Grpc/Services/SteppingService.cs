using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Steps;
using Stepping.TmProviders.Dtm.Grpc.Extensions;
using Stepping.TmProviders.Dtm.Grpc.Secrets;
using Stepping.TmProviders.Dtm.Grpc.Services.Generated;

namespace Stepping.TmProviders.Dtm.Grpc.Services;

public class SteppingService : Generated.SteppingService.SteppingServiceBase
{
    protected IServiceProvider ServiceProvider { get; }
    protected IActionApiTokenChecker ActionApiTokenChecker { get; }

    public SteppingService(
        IServiceProvider serviceProvider,
        IActionApiTokenChecker actionApiTokenChecker)
    {
        ServiceProvider = serviceProvider;
        ActionApiTokenChecker = actionApiTokenChecker;
    }

    public override async Task<Empty> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
    {
        await CheckActionApiTokenAsync(context);

        var stepExecutor = ServiceProvider.GetRequiredService<IStepExecutor>();
        await stepExecutor.ExecuteAsync(request.StepName, request.ArgsToByteString);

        return new Empty();
    }

    public override async Task<Empty> QueryPrepared(Empty request, ServerCallContext context)
    {
        await CheckActionApiTokenAsync(context);

        var gid = context.GetHeader(DtmRequestHeaderNames.DtmGid);

        var dbContextInfoModel = context.CreateDbContextInfoModel();
        var dbContextProviderResolver = ServiceProvider.GetRequiredService<ISteppingDbContextProviderResolver>();
        var dbContextProvider = await dbContextProviderResolver.ResolveAsync(dbContextInfoModel.DbProviderName);
        var dbContext = await dbContextProvider.GetAsync(dbContextInfoModel);

        var barrierInfoModelFactory = ServiceProvider.GetRequiredService<IBarrierInfoModelFactory>();
        var barrierInfoModel = await barrierInfoModelFactory.CreateForRollbackAsync(gid);

        var dbBarrierInserterResolver = ServiceProvider.GetRequiredService<IDbBarrierInserterResolver>();
        var barrierInserter = await dbBarrierInserterResolver.ResolveAsync(dbContextInfoModel.DbProviderName);

        if (await barrierInserter.TryInsertBarrierAsync(barrierInfoModel, dbContext, context.CancellationToken))
        {
            throw new RpcException(new Status(StatusCode.Aborted, DtmConsts.ResultFailure));
        }

        return new Empty();
    }

    protected virtual async Task CheckActionApiTokenAsync(ServerCallContext context)
    {
        var token = context.RequestHeaders.GetValue(DtmRequestHeaderNames.ActionApiToken);

        if (token is null || !await ActionApiTokenChecker.IsCorrectAsync(token))
        {
            throw new SteppingException("Incorrect ActionApiToken!");
        }
    }
}