using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.Dtm.Grpc.Steps;
using Xunit;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Tests;

public class StepToDtmStepConvertResolverTests : SteppingTmProvidersDtmGrpcTestBase
{
    protected IStepToDtmStepConvertResolver StepToDtmStepConvertResolver { get; }

    public StepToDtmStepConvertResolverTests()
    {
        StepToDtmStepConvertResolver = ServiceProvider.GetRequiredService<IStepToDtmStepConvertResolver>();
    }

    [Fact]
    public async Task Should_Resolve_Executable_Step()
    {
        (await StepToDtmStepConvertResolver.ResolveAsync(new FakeExecutableStep())).ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Resolve_Executable_Step_With_Args()
    {
        var step = new FakeWithArgsExecutableStep(new FakeArgs("my-input"));

        (await StepToDtmStepConvertResolver.ResolveAsync(step)).ShouldNotBeNull();
    }
}