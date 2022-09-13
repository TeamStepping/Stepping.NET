using System.Net.Mime;
using System.Text;
using Stepping.Core.Infrastructures;

namespace Stepping.Core.Steps;

[StepName(HttpRequestStepName)]
public class HttpRequestStep : StepWithArgsBase<HttpRequestStepArgs>, IResolveByStep<HttpRequestStep>
{
    public const string HttpRequestStepName = "HttpRequest";

    public HttpRequestStep(HttpRequestStepArgs args) : base(args)
    {
    }

    public virtual HttpRequestMessage CreateHttpRequestMessage(ISteppingJsonSerializer jsonSerializer)
    {
        var message = new HttpRequestMessage(Args.GetHttpMethod(), new Uri(Args.Endpoint, UriKind.Absolute));

        if (message.Method != HttpMethod.Get)
        {
            message.Content = new StringContent(Args.ConvertPayloadToJsonString(jsonSerializer), Encoding.UTF8,
                MediaTypeNames.Application.Json);
        }

        foreach (var header in Args.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return message;
    }
}