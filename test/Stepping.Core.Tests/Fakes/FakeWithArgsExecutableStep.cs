using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Steps;

namespace Stepping.Core.Tests.Fakes;

public record TargetServiceInfoArgs(Type ServiceType);

public class FakeWithArgsExecutableStep : ExecutableStep<TargetServiceInfoArgs>
{
    public const string FakeWithArgsExecutableStepName = "FakeWithArgs";

    public override string StepName => FakeWithArgsExecutableStepName;

    public FakeWithArgsExecutableStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync(TargetServiceInfoArgs args)
    {
        ServiceProvider.GetRequiredService(args.ServiceType);

        return Task.CompletedTask;
    }
}