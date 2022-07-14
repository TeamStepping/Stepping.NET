﻿using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

public record TargetServiceInfoArgs(Type ServiceType);

[StepName(FakeWithArgsExecutableStepName)]
public class FakeWithArgsExecutableStep : ExecutableStep<TargetServiceInfoArgs>
{
    public const string FakeWithArgsExecutableStepName = "FakeWithArgs";

    public FakeWithArgsExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync(TargetServiceInfoArgs args)
    {
        ServiceProvider.GetRequiredService(args.ServiceType);

        return Task.CompletedTask;
    }
}