using System.Text;
using Newtonsoft.Json;
using Stepping.Core.Infrastructures;

namespace Stepping.Core.Steps;

public class JsonStepArgsSerializer : IStepArgsSerializer
{
    private readonly ISteppingJsonSerializer _jsonSerializer;

    public JsonStepArgsSerializer(ISteppingJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public virtual Task<byte[]> SerializeAsync(object obj)
    {
        return Task.FromResult(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))
        );
    }

    public virtual Task<object> DeserializeAsync(byte[] value, Type type)
    {
        return DeserializeAsync(Encoding.UTF8.GetString(value), type);
    }

    public virtual Task<object> DeserializeAsync(string stringValue, Type type)
    {
        return Task.FromResult(_jsonSerializer.Deserialize(type, stringValue));
    }
}