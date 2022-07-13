using Microsoft.Extensions.Options;
using Stepping.Core.Infrastructures;
using Stepping.TmProviders.Dtm.Grpc.Clients;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Steps;
using Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Fakes;

public class FakeDtmGrpcTmClient : DtmGrpcTmClient
{
    public static (string, DtmRequest)? LastInvoking { get; set; }

    public FakeDtmGrpcTmClient(
        IOptions<SteppingDtmGrpcOptions> options,
        ISteppingJsonSerializer jsonSerializer,
        IConnectionStringEncryptor connectionStringEncryptor,
        IStepToDtmStepConvertResolver stepToDtmStepConvertResolver)
        : base(options, jsonSerializer, connectionStringEncryptor, stepToDtmStepConvertResolver)
    {
    }

    protected override Task InvokeDtmServerAsync(string methodName, DtmRequest dtmRequest,
        CancellationToken cancellationToken = default)
    {
        LastInvoking = new ValueTuple<string, DtmRequest>(methodName, dtmRequest);

        return Task.CompletedTask;
    }
}