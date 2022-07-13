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
    protected FakeExecutableStep FakeExecutableStep { get; }
    protected FakeWithArgsExecutableStep FakeWithArgsExecutableStep { get; }
    protected ExecutableStepToDtmStepConverter ExecutableStepToDtmStepConverter { get; }

    public ExecutableStepToDtmStepConverterTests()
    {
        Options = ServiceProvider.GetRequiredService<IOptions<SteppingDtmGrpcOptions>>().Value;
        FakeExecutableStep = ServiceProvider.GetRequiredService<FakeExecutableStep>();
        FakeWithArgsExecutableStep = ServiceProvider.GetRequiredService<FakeWithArgsExecutableStep>();
        ExecutableStepToDtmStepConverter = ServiceProvider.GetRequiredService<ExecutableStepToDtmStepConverter>();
    }

    [Fact]
    public async Task Should_Convert_Executable_Step()
    {
        (await ExecutableStepToDtmStepConverter.CanConvertAsync(FakeExecutableStep)).ShouldBeTrue();

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
        (await ExecutableStepToDtmStepConverter.CanConvertAsync(FakeWithArgsExecutableStep)).ShouldBeTrue();

        var stepInfoModel =
            await ExecutableStepToDtmStepConverter.ConvertAsync(
                FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName,
                new TargetServiceInfoArgs(typeof(FakeService)));

        stepInfoModel.Step.Count.ShouldBe(1);
        stepInfoModel.Step.ShouldContainKey(DtmConsts.ActionStepName);
        stepInfoModel.Step[DtmConsts.ActionStepName].ShouldBe(Options.GetExecuteStepAddress());
        stepInfoModel.BinPayload.ShouldNotBeNull();
    }
}