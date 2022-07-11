using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Stepping.Core;
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
    protected static readonly Marshaller<DtmRequest> DtmRequestMarshaller =
        Marshallers.Create(r => r.ToByteArray(), data => DtmRequest.Parser.ParseFrom(data));

    protected static readonly Marshaller<Empty> DtmReplyMarshaller =
        Marshallers.Create(r => r.ToByteArray(), data => Empty.Parser.ParseFrom(data));

    protected string DtmServiceName { get; set; } = "dtm.DtmServer";

    protected SteppingDtmGrpcOptions Options { get; }
    protected ISteppingJsonSerializer JsonSerializer { get; }
    protected IConnectionStringEncryptor ConnectionStringEncryptor { get; }
    protected IStepToDtmStepConvertResolver StepToDtmStepConvertResolver { get; }

    public DtmGrpcTmClient(
        IOptions<SteppingDtmGrpcOptions> options,
        ISteppingJsonSerializer jsonSerializer,
        IConnectionStringEncryptor connectionStringEncryptor,
        IStepToDtmStepConvertResolver stepToDtmStepConvertResolver)
    {
        Options = options.Value;
        JsonSerializer = jsonSerializer;
        ConnectionStringEncryptor = connectionStringEncryptor;
        StepToDtmStepConvertResolver = stepToDtmStepConvertResolver;
    }

    public virtual async Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        await AddDbContextInfoHeadersAsync(job);

        var dtmRequest = await BuildDtmRequestAsync(job);

        await InvokeDtmServerAsync("Prepare", dtmRequest, cancellationToken);
    }

    public virtual async Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        var dtmRequest = await BuildDtmRequestAsync(job);

        await InvokeDtmServerAsync("Submit", dtmRequest, cancellationToken);
    }

    protected virtual async Task AddDbContextInfoHeadersAsync(IDistributedJob job)
    {
        if (job.DbTransactionContext is null)
        {
            return;
        }

        var headers = job.GetDtmJobConfigurations().BranchHeaders;
        var dbProviderName = job.DbTransactionContext.DbContext.DbProviderName;

        var dbContextType = job.DbTransactionContext.DbContext.GetInternalDbContextTypeOrNull();
        var dbContextTypeName = dbContextType is null
            ? string.Empty
            : $"{dbContextType.FullName}, {dbContextType.Assembly.GetName().Name}";

        var databaseName = job.DbTransactionContext.DbContext.GetInternalDatabaseNameOrNull() ?? string.Empty;

        var encryptedConnectionString =
            await ConnectionStringEncryptor.EncryptAsync(job.DbTransactionContext.DbContext.ConnectionString);

        headers.Add(DtmRequestHeaderNames.DbProviderName, dbProviderName);
        headers.Add(DtmRequestHeaderNames.DbContextType, dbContextTypeName);
        headers.Add(DtmRequestHeaderNames.Database, databaseName);
        headers.Add(DtmRequestHeaderNames.EncryptedConnectionString, encryptedConnectionString);
    }

    protected virtual async Task InvokeDtmServerAsync(string methodName, DtmRequest dtmRequest,
        CancellationToken cancellationToken = default)
    {
        var method = new Method<DtmRequest, Empty>(
            MethodType.Unary, DtmServiceName, methodName, DtmRequestMarshaller, DtmReplyMarshaller);

        using var channel = GrpcChannel.ForAddress(Options.DtmGrpcUrl);

        var callOptions = new CallOptions()
            .WithCancellationToken(cancellationToken)
            .WithDeadline(DateTime.UtcNow.AddMilliseconds(Options.DtmServerRequestTimeout));

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