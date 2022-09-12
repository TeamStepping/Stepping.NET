﻿using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.LocalTm.Steps;
using Xunit;

namespace Stepping.TmProviders.LocalTm.Core.Tests.Tests;

public class LocalTmStepConverterTests : SteppingTmProvidersLocalTmCoreTestBase
{
    protected ILocalTmStepConverter LocalTmStepConverter { get; }

    protected IStepArgsSerializer StepArgsSerializer { get; }

    public LocalTmStepConverterTests()
    {
        LocalTmStepConverter = ServiceProvider.GetRequiredService<ILocalTmStepConverter>();
        StepArgsSerializer = ServiceProvider.GetRequiredService<IStepArgsSerializer>();
    }

    [Fact]
    public async Task Should_Convert_Steps_To_Model()
    {
        var fakeWithArgsExecutableStep = new FakeWithArgsExecutableStep(new FakeArgs("my-input"));

        var steps = new List<IStep>
        {
            new FakeExecutableStep(),
            fakeWithArgsExecutableStep
        };

        var model = await LocalTmStepConverter.ConvertAsync(steps);

        model.Steps.Count.ShouldBe(2);
        model.Steps[0].StepName.ShouldBe(FakeExecutableStep.FakeExecutableStepName);
        model.Steps[0].ArgsToByteString.ShouldBeNull();
        model.Steps[0].Executed.ShouldBe(false);

        model.Steps[1].StepName.ShouldBe(FakeWithArgsExecutableStep.FakeWithArgsExecutableStepName);
        model.Steps[1].ArgsToByteString.ShouldBe(
            Encoding.UTF8.GetString(
                await StepArgsSerializer.SerializeAsync(fakeWithArgsExecutableStep.GetArgs())
            )
        );
        model.Steps[1].Executed.ShouldBe(false);
    }
}
