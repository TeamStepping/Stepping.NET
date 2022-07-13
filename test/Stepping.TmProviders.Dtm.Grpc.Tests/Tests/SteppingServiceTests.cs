using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Infrastructures;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.Dtm.Grpc.Secrets;
using Stepping.TmProviders.Dtm.Grpc.Services.Generated;
using Xunit;
using SteppingService = Stepping.TmProviders.Dtm.Grpc.Services.SteppingService;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Tests;

public class SteppingServiceTests : SteppingTmProvidersDtmGrpcTestBase
{
    protected IConnectionStringEncryptor ConnectionStringEncryptor { get; }
    protected SteppingService SteppingService { get; }

    public SteppingServiceTests()
    {
        ConnectionStringEncryptor = ServiceProvider.GetRequiredService<IConnectionStringEncryptor>();
        SteppingService =
            new SteppingService(ServiceProvider, ServiceProvider.GetRequiredService<IActionApiTokenChecker>());
    }

    [Fact]
    public async Task Should_Invoke_ExecuteStep()
    {
        var serverCallContext = CreateTestServerCallContext(nameof(SteppingService.ExecuteStep));

        await Should.NotThrowAsync(() => SteppingService.ExecuteStep(new ExecuteStepRequest
        {
            StepName = FakeExecutableStep.FakeExecutableStepName,
            ArgsToByteString = string.Empty
        }, serverCallContext));
    }

    [Fact]
    public async Task Should_Invoke_QueryPrepared()
    {
        var serverCallContext = CreateTestServerCallContext(nameof(SteppingService.ExecuteStep));

        serverCallContext.RequestHeaders.Add(DtmRequestHeaderNames.DtmGid, Guid.NewGuid().ToString());
        serverCallContext.RequestHeaders.Add(DtmRequestHeaderNames.DbProviderName,
            FakeSteppingDbContext.FakeDbProviderName);
        serverCallContext.RequestHeaders.Add(DtmRequestHeaderNames.DbContextType, string.Empty);
        serverCallContext.RequestHeaders.Add(DtmRequestHeaderNames.Database, string.Empty);
        serverCallContext.RequestHeaders.Add(DtmRequestHeaderNames.EncryptedConnectionString,
            await ConnectionStringEncryptor.EncryptAsync(FakeSteppingDbContext.FakeConnectionString));

        // It throws since the barrier with "rollback" is successfully inserted.
        await Should.ThrowAsync<RpcException>(() => SteppingService.QueryPrepared(new Empty(), serverCallContext),
            "Status(StatusCode=\"Aborted\", Detail=\"FAILURE\")");
    }

    private static ServerCallContext CreateTestServerCallContext(string method)
    {
        return TestServerCallContext.Create(
            method: method,
            host: "localhost",
            deadline: DateTime.Now.AddMinutes(30),
            requestHeaders: new Metadata
            {
                { DtmRequestHeaderNames.ActionApiToken, "" }
            },
            cancellationToken: CancellationToken.None,
            peer: "10.0.0.25:5001",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: (_) => Task.CompletedTask,
            writeOptionsGetter: () => new WriteOptions(),
            writeOptionsSetter: (_) => { }
        );
    }
}