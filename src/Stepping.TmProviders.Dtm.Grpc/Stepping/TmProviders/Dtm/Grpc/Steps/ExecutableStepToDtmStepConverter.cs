using System.Text;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Stepping.Core.Steps;
using Stepping.TmProviders.Dtm.Grpc.Options;
using Stepping.TmProviders.Dtm.Grpc.Services.Generated;

namespace Stepping.TmProviders.Dtm.Grpc.Steps;

public class ExecutableStepToDtmStepConverter : IStepToDtmStepConverter
{
    protected IStepArgsSerializer StepArgsSerializer { get; }
    protected SteppingDtmGrpcOptions Options { get; }

    public ExecutableStepToDtmStepConverter(
        IStepArgsSerializer stepArgsSerializer,
        IOptions<SteppingDtmGrpcOptions> options)
    {
        StepArgsSerializer = stepArgsSerializer;
        Options = options.Value;
    }

    public virtual Task<bool> CanConvertAsync(IStep step)
    {
        return Task.FromResult(step is ExecutableStep);
    }

    public virtual async Task<DtmStepInfoModel> ConvertAsync(string stepName, object? args)
    {
        var argsToByteString = args is not null
            ? Encoding.UTF8.GetString(await StepArgsSerializer.SerializeAsync(args))
            : null;

        var payload = new ExecuteStepRequest
        {
            StepName = stepName,
            ArgsToByteString = argsToByteString
        };

        return new DtmStepInfoModel(
            new Dictionary<string, string>
            {
                { DtmConsts.ActionStepName, Options.GetExecuteStepPathAddress() }
            },
            payload.ToByteString()
        );
    }
}