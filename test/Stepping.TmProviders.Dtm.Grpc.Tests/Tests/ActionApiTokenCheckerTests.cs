using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Secrets;
using Stepping.TmProviders.Dtm.Grpc.Tests.Fakes;
using Xunit;

namespace Stepping.TmProviders.Dtm.Grpc.Tests.Tests;

public class ActionApiTokenCheckerTests : SteppingTmProvidersDtmGrpcTestBase
{
    protected IActionApiTokenChecker ActionApiTokenChecker { get; }

    public ActionApiTokenCheckerTests()
    {
        ActionApiTokenChecker = ServiceProvider.GetRequiredService<IActionApiTokenChecker>();
    }

    [Fact]
    public async Task Should_Check_Token()
    {
        FakeDefaultActionApiTokenChecker.FakeOptions.ActionApiToken = "correct-token";

        (await ActionApiTokenChecker.IsCorrectAsync("correct-token")).ShouldBeTrue();
        (await ActionApiTokenChecker.IsCorrectAsync("invalid-token")).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Always_Be_Correct_If_Token_In_Options_Is_Null_Or_Empty()
    {
        FakeDefaultActionApiTokenChecker.FakeOptions.ActionApiToken = null;

        (await ActionApiTokenChecker.IsCorrectAsync(null)).ShouldBeTrue();
        (await ActionApiTokenChecker.IsCorrectAsync("")).ShouldBeTrue();
        (await ActionApiTokenChecker.IsCorrectAsync("some-token")).ShouldBeTrue();
        
        FakeDefaultActionApiTokenChecker.FakeOptions.ActionApiToken = "";

        (await ActionApiTokenChecker.IsCorrectAsync(null)).ShouldBeTrue();
        (await ActionApiTokenChecker.IsCorrectAsync("")).ShouldBeTrue();
        (await ActionApiTokenChecker.IsCorrectAsync("some-token")).ShouldBeTrue();
    }
}