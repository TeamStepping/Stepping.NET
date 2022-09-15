using Google.Protobuf;
using Microsoft.Extensions.Options;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.Core.Steps;
using Stepping.TmProviders.Dtm.Grpc.Options;

namespace Stepping.TmProviders.Dtm.Grpc.Steps;

public class HttpRequestStepToDtmStepConverter : IStepToDtmStepConverter
{
    protected IStepResolver StepResolver { get; }
    protected ISteppingJsonSerializer JsonSerializer { get; }
    protected SteppingDtmGrpcOptions Options { get; }

    public HttpRequestStepToDtmStepConverter(
        IStepResolver stepResolver,
        ISteppingJsonSerializer jsonSerializer,
        IOptions<SteppingDtmGrpcOptions> options)
    {
        StepResolver = stepResolver;
        JsonSerializer = jsonSerializer;
        Options = options.Value;
    }

    public virtual Task<bool> CanConvertAsync(IStep step)
    {
        return Task.FromResult(step is HttpRequestStep);
    }

    public virtual Task<DtmStepInfoModel> ConvertAsync(string stepName, object? args)
    {
        var step = (HttpRequestStep)StepResolver.Resolve(stepName, args);
        var requestArgs = step.Args;

        if (requestArgs.HttpMethod != HttpMethod.Get && requestArgs.HttpMethod != HttpMethod.Post)
        {
            throw new SteppingException("DTM support only GET and POST methods for HTTP request.");
        }

        if (requestArgs.HttpMethod == HttpMethod.Get && requestArgs.Payload.Any())
        {
            throw new SteppingException("DTM doesn't support GET with payload.");
        }

        if (requestArgs.Headers.Any())
        {
            throw new SteppingException(
                "You cannot set headers for HttpRequestStep when using DTM. Please use `job.SetBranchHeader(name, value)` instead.");
        }

        return Task.FromResult(new DtmStepInfoModel(
            new Dictionary<string, string>
            {
                { DtmConsts.ActionStepName, requestArgs.Endpoint }
            },
            requestArgs.HttpMethod != HttpMethod.Get
                ? ByteString.CopyFromUtf8(requestArgs.ConvertPayloadToJsonString(JsonSerializer))
                : ByteString.Empty
        ));
    }
}