using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Stepping.Core;
using Stepping.Core.Databases;
using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.Dtm.Grpc.Extensions;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Tests.Fakes;
using Xunit;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Tests;

public class DtmGrpcTmClientTests : SteppingTmProvidersDtmGrpcTestBase
{
    protected SteppingDtmGrpcOptions Options { get; }
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected IAtomicJobFactory AtomicJobFactory { get; }
    protected FakeDtmGrpcTmClient FakeDtmGrpcTmClient { get; }

    public DtmGrpcTmClientTests()
    {
        Options = ServiceProvider.GetRequiredService<IOptions<SteppingDtmGrpcOptions>>().Value;
        ConnectionStringHasher = ServiceProvider.GetRequiredService<IConnectionStringHasher>();
        AtomicJobFactory = ServiceProvider.GetRequiredService<IAtomicJobFactory>();
        FakeDtmGrpcTmClient = (FakeDtmGrpcTmClient)ServiceProvider.GetRequiredService<ITmClient>();
    }

    [Fact]
    public async Task Should_Send_Prepare()
    {
        var job = await AtomicJobFactory.CreateJobAsync(Guid.NewGuid().ToString(),
            new FakeSteppingDbContext(true, "some-info"));

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        job.SetBranchHeader("header1", "header1_value");
        job.SetWaitResult(true);
        job.SetRetryInterval(123);
        job.SetTimeoutToFail(456);
        job.SetRetryLimit(100);

        await FakeDtmGrpcTmClient.PrepareAsync(job);

        FakeDtmGrpcTmClient.LastInvoking.ShouldNotBeNull();
        FakeDtmGrpcTmClient.LastInvoking.Value.Item1.ShouldBe("PrepareAsync");
        FakeDtmGrpcTmClient.LastInvoking.Value.Item2.ShouldNotBeNull();

        var dtmRequest = FakeDtmGrpcTmClient.LastInvoking.Value.Item2;
        var address = Options.GetExecuteStepAddress();
        dtmRequest.Gid.ShouldBe(job.Gid);
        dtmRequest.Steps.ShouldBe($"[{{\"action\":\"{address}\"}},{{\"action\":\"{address}\"}}]");
        dtmRequest.BinPayloads.Count.ShouldBe(2);
        dtmRequest.BinPayloads.ShouldNotContain(x => x.IsEmpty);
        dtmRequest.CustomedData.ShouldBeEmpty();
        dtmRequest.QueryPrepared.ShouldBe(Options.GetQueryPreparedAddress());
        dtmRequest.ReqExtra.ShouldBeEmpty();
        dtmRequest.RollbackReason.ShouldBeEmpty();
        dtmRequest.TransType.ShouldBe(SteppingConsts.TypeMsg);

        var headers = dtmRequest.TransOptions.BranchHeaders;

        headers.ShouldContainKey("header1");
        headers.ShouldContainKey(DtmRequestHeaderNames.DbProviderName);
        headers.ShouldContainKey(DtmRequestHeaderNames.HashedConnectionString);
        headers.ShouldContainKey(DtmRequestHeaderNames.DbContextType);
        headers.ShouldContainKey(DtmRequestHeaderNames.Database);
        headers.ShouldContainKey(DtmRequestHeaderNames.TenantId);
        headers.ShouldContainKey(DtmRequestHeaderNames.CustomInfo);

        headers["header1"].ShouldBe("header1_value");
        headers[DtmRequestHeaderNames.DbProviderName].ShouldBe(job.DbContext!.DbProviderName);
        headers[DtmRequestHeaderNames.HashedConnectionString]
            .ShouldBe(await ConnectionStringHasher.HashAsync(FakeSteppingDbContext.FakeConnectionString));
        headers[DtmRequestHeaderNames.DbContextType].ShouldBe(string.Empty);
        headers[DtmRequestHeaderNames.Database].ShouldBe(string.Empty);
        headers[DtmRequestHeaderNames.TenantId].ShouldBe(string.Empty);
        headers[DtmRequestHeaderNames.CustomInfo].ShouldBe("some-info");

        dtmRequest.TransOptions.RetryInterval.ShouldBe(123);
        dtmRequest.TransOptions.TimeoutToFail.ShouldBe(456);
        dtmRequest.TransOptions.RetryLimit.ShouldBe(100);
        dtmRequest.TransOptions.WaitResult.ShouldBeTrue();
        dtmRequest.TransOptions.RequestTimeout.ShouldBe(Options.BranchRequestTimeout);
    }

    [Fact]
    public async Task Should_Send_Submit()
    {
        var job = await AtomicJobFactory.CreateJobAsync(Guid.NewGuid().ToString(),
            new FakeSteppingDbContext(true));

        job.AddStep<FakeExecutableStep>();
        job.AddStep(new FakeWithArgsExecutableStep(new FakeArgs("my-input")));

        job.SetBranchHeader("header1", "header1_value");
        job.SetWaitResult(true);
        job.SetRetryInterval(123);
        job.SetTimeoutToFail(456);
        job.SetRetryLimit(100);

        await FakeDtmGrpcTmClient.SubmitAsync(job);

        FakeDtmGrpcTmClient.LastInvoking.ShouldNotBeNull();
        FakeDtmGrpcTmClient.LastInvoking.Value.Item1.ShouldBe("SubmitAsync");
        FakeDtmGrpcTmClient.LastInvoking.Value.Item2.ShouldNotBeNull();

        var dtmRequest = FakeDtmGrpcTmClient.LastInvoking.Value.Item2;
        var address = Options.GetExecuteStepAddress();
        dtmRequest.Gid.ShouldBe(job.Gid);
        dtmRequest.Steps.ShouldBe($"[{{\"action\":\"{address}\"}},{{\"action\":\"{address}\"}}]");
        dtmRequest.BinPayloads.Count.ShouldBe(2);
        dtmRequest.BinPayloads.ShouldNotContain(x => x.IsEmpty);
        dtmRequest.CustomedData.ShouldBeEmpty();
        dtmRequest.QueryPrepared.ShouldBe(Options.GetQueryPreparedAddress());
        dtmRequest.ReqExtra.ShouldBeEmpty();
        dtmRequest.RollbackReason.ShouldBeEmpty();
        dtmRequest.TransType.ShouldBe(SteppingConsts.TypeMsg);

        var headers = dtmRequest.TransOptions.BranchHeaders;

        headers.ShouldContainKey("header1");

        headers["header1"].ShouldBe("header1_value");

        dtmRequest.TransOptions.RetryInterval.ShouldBe(123);
        dtmRequest.TransOptions.TimeoutToFail.ShouldBe(456);
        dtmRequest.TransOptions.RetryLimit.ShouldBe(100);
        dtmRequest.TransOptions.WaitResult.ShouldBeTrue();
        dtmRequest.TransOptions.RequestTimeout.ShouldBe(Options.BranchRequestTimeout);
    }
}