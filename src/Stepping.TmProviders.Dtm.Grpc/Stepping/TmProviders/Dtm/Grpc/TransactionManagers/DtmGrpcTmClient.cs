using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Stepping.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.Dtm.Grpc.Clients;
using Stepping.TmProviders.Dtm.Grpc.Extensions;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Steps;

namespace Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

public class DtmGrpcTmClient : ITmClient
{
    protected SteppingDtmGrpcOptions Options { get; }
    protected ISteppingJsonSerializer JsonSerializer { get; }
    protected Clients.Dtm.DtmClient DtmClient { get; }

    protected IStepToDtmStepConvertResolver StepToDtmStepConvertResolver { get; }
    protected ISteppingDbContextLookupInfoProvider DbContextLookupInfoProvider { get; }

    public DtmGrpcTmClient(
        IOptions<SteppingDtmGrpcOptions> options,
        ISteppingJsonSerializer jsonSerializer,
        Clients.Dtm.DtmClient dtmClient,
        IStepToDtmStepConvertResolver stepToDtmStepConvertResolver,
        ISteppingDbContextLookupInfoProvider dbContextLookupInfoProvider)
    {
        Options = options.Value;
        JsonSerializer = jsonSerializer;
        DtmClient = dtmClient;
        StepToDtmStepConvertResolver = stepToDtmStepConvertResolver;
        DbContextLookupInfoProvider = dbContextLookupInfoProvider;
    }

    public virtual async Task PrepareAsync(IAtomicJob job, CancellationToken cancellationToken = default)
    {
        if (job.PrepareSent)
        {
            throw new SteppingException("Duplicate sending prepare to TM.");
        }

        if (job.DbContext is not null)
        {
            await AddDbContextLookupInfoHeadersAsync(job);
        }

        await InvokeDtmServerAsync(DtmClient.PrepareAsync, await BuildDtmRequestAsync(job), cancellationToken);
    }

    protected delegate AsyncUnaryCall<TResult> GrpcMethod<TResult>(DtmRequest dtmRequest, CallOptions callOptions);

    protected virtual async Task InvokeDtmServerAsync<TResult>(GrpcMethod<TResult> method, DtmRequest dtmRequest,
        CancellationToken cancellationToken = default)
    {
        await method(dtmRequest, CreateGrpcCallOptions(cancellationToken));
    }

    public virtual async Task SubmitAsync(IAtomicJob job, CancellationToken cancellationToken = default)
    {
        if (job.SubmitSent)
        {
            throw new SteppingException("Duplicate sending submit to TM.");
        }

        await InvokeDtmServerAsync(DtmClient.SubmitAsync, await BuildDtmRequestAsync(job), cancellationToken);
    }

    protected virtual async Task AddDbContextLookupInfoHeadersAsync(IAtomicJob job)
    {
        var headers = job.GetDtmJobConfigurations().BranchHeaders;

        var lookupInfoModel = await DbContextLookupInfoProvider.GetAsync(job.DbContext!);

        headers.Add(DtmRequestHeaderNames.DbProviderName, lookupInfoModel.DbProviderName);
        headers.Add(DtmRequestHeaderNames.HashedConnectionString, lookupInfoModel.HashedConnectionString);
        headers.Add(DtmRequestHeaderNames.DbContextType, lookupInfoModel.DbContextType ?? string.Empty);
        headers.Add(DtmRequestHeaderNames.Database, lookupInfoModel.Database ?? string.Empty);
        headers.Add(DtmRequestHeaderNames.TenantId, lookupInfoModel.TenantId ?? string.Empty);
        headers.Add(DtmRequestHeaderNames.CustomInfo, lookupInfoModel.CustomInfo ?? string.Empty);
    }

    protected virtual CallOptions CreateGrpcCallOptions(CancellationToken cancellationToken = default)
    {
        return new CallOptions()
            .WithCancellationToken(cancellationToken)
            .WithDeadline(DateTime.UtcNow.AddMilliseconds(Options.DtmServerRequestTimeout));
    }

    protected virtual async Task<DtmRequest> BuildDtmRequestAsync(IAtomicJob job)
    {
        var configurations = job.GetDtmJobConfigurations();

        if (Options.ActionApiToken is not null or "")
        {
            configurations.BranchHeaders.TryAdd(DtmRequestHeaderNames.ActionApiToken, Options.ActionApiToken);
        }

        var transOptions = new DtmTransOptions
        {
            WaitResult = configurations.WaitResult,
            TimeoutToFail = configurations.TimeoutToFail,
            RetryInterval = configurations.RetryInterval,
            RequestTimeout = Options.BranchRequestTimeout
        };

        transOptions.BranchHeaders.Add(configurations.BranchHeaders);
        transOptions.PassthroughHeaders.Add(configurations.PassthroughHeaders);

        var dtmSteps = new List<Dictionary<string, string>>();
        var dtmBinPayloads = new List<ByteString>();

        foreach (var step in job.Steps)
        {
            var dtmStepInfoModel = await StepToDtmStepConvertResolver.ResolveAsync(step);

            dtmSteps.Add(dtmStepInfoModel.Step);
            dtmBinPayloads.Add(dtmStepInfoModel.BinPayload);
        }

        var dtmRequest = new DtmRequest
        {
            Gid = job.Gid,
            TransType = SteppingConsts.TypeMsg,
            TransOptions = transOptions,
            QueryPrepared = Options.GetQueryPreparedAddress(),
            CustomedData = string.Empty,
            Steps = dtmSteps.Count == 0 ? string.Empty : JsonSerializer.Serialize(dtmSteps)
        };

        foreach (var dtmBinPayload in dtmBinPayloads)
        {
            dtmRequest.BinPayloads.Add(dtmBinPayload);
        }

        return dtmRequest;
    }
}