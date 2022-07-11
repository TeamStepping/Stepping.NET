using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Stepping.Core;
using Stepping.Core.Infrastructures;
using Stepping.Core.Jobs;
using Stepping.Core.Steps;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.Dtm.Grpc.Clients;
using Stepping.TmProviders.Dtm.Grpc.Extensions;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Steps;

namespace Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

public class DtmGrpcTmClient : ITmClient
{
    protected static readonly Marshaller<DtmRequest> DtmRequestMarshaller =
        Marshallers.Create(r => r.ToByteArray(), data => DtmRequest.Parser.ParseFrom(data));

    protected static readonly Marshaller<Empty> DtmReplyMarshaller =
        Marshallers.Create(r => r.ToByteArray(), data => Empty.Parser.ParseFrom(data));

    protected string DtmServiceName { get; set; } = "dtm.DtmServer";

    protected SteppingDtmGrpcOptions Options { get; }
    protected ISteppingJsonSerializer JsonSerializer { get; }
    protected IStepToDtmStepConvertResolver StepToDtmStepConvertResolver { get; }

    public DtmGrpcTmClient(
        IOptions<SteppingDtmGrpcOptions> options,
        ISteppingJsonSerializer jsonSerializer,
        IStepToDtmStepConvertResolver stepToDtmStepConvertResolver)
    {
        Options = options.Value;
        JsonSerializer = jsonSerializer;
        StepToDtmStepConvertResolver = stepToDtmStepConvertResolver;
    }

    public virtual Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        return InvokeDtmServerAsync(job, "Prepare", cancellationToken);
    }

    public virtual Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        return InvokeDtmServerAsync(job, "Submit", cancellationToken);
    }

    protected virtual async Task InvokeDtmServerAsync(IDistributedJob job, string methodName,
        CancellationToken cancellationToken = default)
    {
        var dtmRequest = await BuildDtmRequestAsync(job);

        var method = new Method<DtmRequest, Empty>(
            MethodType.Unary, DtmServiceName, methodName, DtmRequestMarshaller, DtmReplyMarshaller);

        using var channel = GrpcChannel.ForAddress(Options.DtmGrpcUrl);

        var callOptions =
            new CallOptions().WithDeadline(DateTime.UtcNow.AddMilliseconds(Options.DtmServerRequestTimeout));

        await channel.CreateCallInvoker().AsyncUnaryCall(method, null, callOptions, dtmRequest);
    }

    protected virtual async Task<DtmRequest> BuildDtmRequestAsync(IDistributedJob job)
    {
        var configurations = job.GetDtmJobConfigurations();

        var transOptions = new DtmTransOptions
        {
            WaitResult = true,
            RetryInterval = configurations.RetryInterval,
            TimeoutToFail = configurations.TimeoutToFail,
            RequestTimeout = Options.BranchRequestTimeout
        };

        transOptions.BranchHeaders.Add(configurations.BranchHeaders);
        transOptions.PassthroughHeaders.Add(configurations.PassthroughHeaders);

        var dtmSteps = new List<Dictionary<string, string>>();
        var dtmBinPayloads = new List<ByteString>();

        foreach (var stepInfoModel in job.Steps)
        {
            var dtmStepInfoModel =
                await StepToDtmStepConvertResolver.ResolveAsync(stepInfoModel.StepName, stepInfoModel.Args);

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