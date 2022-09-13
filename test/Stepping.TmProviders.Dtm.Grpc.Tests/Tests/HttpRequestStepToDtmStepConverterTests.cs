using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Stepping.Core.Exceptions;
using Stepping.Core.Steps;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Steps;
using Xunit;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Tests;

public class HttpRequestStepToDtmStepConverterTests : SteppingTmProvidersDtmGrpcTestBase
{
    protected SteppingDtmGrpcOptions Options { get; }
    protected HttpRequestStepToDtmStepConverter HttpRequestStepToDtmStepConverter { get; }

    public HttpRequestStepToDtmStepConverterTests()
    {
        Options = ServiceProvider.GetRequiredService<IOptions<SteppingDtmGrpcOptions>>().Value;
        HttpRequestStepToDtmStepConverter = ServiceProvider.GetRequiredService<HttpRequestStepToDtmStepConverter>();
    }

    [Fact]
    public async Task Should_Convert_Http_Request_Step_With_Http_Get()
    {
        var step = new RequestGitHubGetOrganizationStep("stepping");

        (await HttpRequestStepToDtmStepConverter.CanConvertAsync(step)).ShouldBeTrue();

        var stepInfoModel = await HttpRequestStepToDtmStepConverter.ConvertAsync(
            RequestGitHubGetOrganizationStep.RequestGitHubGetOrganizationStepName, step.Args);

        stepInfoModel.Step.Count.ShouldBe(1);
        stepInfoModel.Step.ShouldContainKey(DtmConsts.ActionStepName);
        stepInfoModel.Step[DtmConsts.ActionStepName].ShouldBe("https://api.github.com/orgs/stepping");
        stepInfoModel.BinPayload.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Convert_Http_Request_Step_With_Http_Post()
    {
        var step = new RequestGitHubRenderMarkdownStep("Hello, world.");

        (await HttpRequestStepToDtmStepConverter.CanConvertAsync(step)).ShouldBeTrue();

        var stepInfoModel = await HttpRequestStepToDtmStepConverter.ConvertAsync(
            RequestGitHubRenderMarkdownStep.RequestGitHubRenderMarkdownName, step.Args);

        stepInfoModel.Step.Count.ShouldBe(1);
        stepInfoModel.Step.ShouldContainKey(DtmConsts.ActionStepName);
        stepInfoModel.Step[DtmConsts.ActionStepName].ShouldBe("https://api.github.com/markdown");
        stepInfoModel.BinPayload.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Convert_Http_Request_Step_With_Http_Post_And_Empty_Payload()
    {
        var args = new HttpRequestStepArgs("https://fakeurl.com", HttpMethod.Post);
        var step = new HttpRequestStep(args);

        (await HttpRequestStepToDtmStepConverter.CanConvertAsync(step)).ShouldBeTrue();

        var stepInfoModel =
            await HttpRequestStepToDtmStepConverter.ConvertAsync(HttpRequestStep.HttpRequestStepName, args);

        stepInfoModel.Step.Count.ShouldBe(1);
        stepInfoModel.Step.ShouldContainKey(DtmConsts.ActionStepName);
        stepInfoModel.Step[DtmConsts.ActionStepName].ShouldBe("https://fakeurl.com");
        stepInfoModel.BinPayload.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Not_Convert_Http_Request_Step_With_Http_Delete()
    {
        var args = new HttpRequestStepArgs("https://fakeurl.com", HttpMethod.Delete);

        await Should.ThrowAsync<SteppingException>(
            () => HttpRequestStepToDtmStepConverter.ConvertAsync(HttpRequestStep.HttpRequestStepName, args),
            "DTM support only GET and POST methods for HTTP request.");
    }
}