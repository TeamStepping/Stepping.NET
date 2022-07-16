using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Steps;
using Xunit;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Tests;

public class ExecutableStepToDtmStepConverterTests : SteppingTmProvidersDtmGrpcTestBase
{
    protected SteppingDtmGrpcOptions Options { get; }
    protected ExecutableStepToDtmStepConverter ExecutableStepToDtmStepConverter { get; }

    public ExecutableStepToDtmStepConverterTests()
    {
        Options = ServiceProvider.GetRequiredService<IOptions<SteppingDtmGrpcOptions>>().Value;
        ExecutableStepToDtmStepConverter = ServiceProvider.GetRequiredService<ExecutableStepToDtmStepConverter>();
    }

    [Fact]
    public async Task Should_Convert_Executable_Step()
    {
        (await ExecutableStepToDtmStepConverter.CanConvertAsync(new FakeExecutableStep())).ShouldBeTrue();

        var stepInfoModel =
            await ExecutableStepToDtmStepConverter.ConvertAsync(FakeExecutableStep.FakeExecutableStepName, null);

        stepInfoModel.Step.Count.ShouldBe(1);
        stepInfoModel.Step.ShouldContainKey(DtmConsts.ActionStepName);
        stepInfoModel.Step[DtmConsts.ActionStepName].ShouldBe(Options.GetExecuteStepAddress());
        stepInfoModel.BinPayload.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Convert_Executable_Step_With_Args()
    {
        (await ExecutableStepToDtmStepConverter.CanConvertAsync(new FakeWithArgsExecutableStep("my-input")))
            .ShouldBeTrue();

        var stepInfoModel =
            await ExecutableStepToDtmStepConverter.ConvertAsync(
                FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName, "my-input");

        stepInfoModel.Step.Count.ShouldBe(1);
        stepInfoModel.Step.ShouldContainKey(DtmConsts.ActionStepName);
        stepInfoModel.Step[DtmConsts.ActionStepName].ShouldBe(Options.GetExecuteStepAddress());
        stepInfoModel.BinPayload.ShouldNotBeNull();
    }
}