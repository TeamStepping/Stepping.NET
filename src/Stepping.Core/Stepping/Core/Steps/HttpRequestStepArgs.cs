using Stepping.Core.Infrastructures;

namespace Stepping.Core.Steps;

public class HttpRequestStepArgs
{
    /// <summary>
    /// The endpoint URL.
    /// It should contain the query parameters you need, for example "https://192.168.1.1/hello?target=world".
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// HTTP method of the request. 
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    /// It will convert to raw data in JSON format.
    /// The request becomes a POST request if the Payload is not empty due to the DTM's design.
    /// </summary>
    public Dictionary<string, object> Payload { get; }

    /// <summary>
    /// Http request headers.
    /// Don't override the headers used by Stepping. If you do so, you will get exception throws.
    /// </summary>
    public List<KeyValuePair<string, string>> Headers { get; }

    public HttpMethod GetHttpMethod() => Payload.Any() ? HttpMethod.Post : HttpMethod.Get;

    public HttpRequestStepArgs(
        string endpoint,
        HttpMethod httpMethod,
        Dictionary<string, object>? payload = null,
        List<KeyValuePair<string, string>>? headers = null)
    {
        Endpoint = endpoint;
        HttpMethod = httpMethod;
        Payload = payload ?? new Dictionary<string, object>();
        Headers = headers ?? new List<KeyValuePair<string, string>>();
    }

    public virtual string ConvertPayloadToJsonString(ISteppingJsonSerializer jsonSerializer) =>
        jsonSerializer.Serialize(Payload);
}