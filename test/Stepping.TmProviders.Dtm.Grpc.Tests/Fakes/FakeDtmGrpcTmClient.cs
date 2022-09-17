using System.Reflection;
using Microsoft.Extensions.Options;
using Stepping.Core.Databases;
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
        Clients.Dtm.DtmClient dtmClient,
        IStepToDtmStepConvertResolver stepToDtmStepConvertResolver,
        ISteppingDbContextLookupInfoProvider dbContextLookupInfoProvider)
        : base(options, jsonSerializer, dtmClient, stepToDtmStepConvertResolver, dbContextLookupInfoProvider)
    {
    }

    protected override Task InvokeDtmServerAsync<TResult>(GrpcMethod<TResult> method, DtmRequest dtmRequest,
        CancellationToken cancellationToken = default)
    {
        LastInvoking = new ValueTuple<string, DtmRequest>(method.GetMethodInfo()!.Name, dtmRequest);

        return Task.CompletedTask;
    }
}