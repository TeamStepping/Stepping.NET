using System.Text;
using Newtonsoft.Json;

namespace Stepping.Core.Steps;

public class JsonStepArgsSerializer : IStepArgsSerializer
{
    public virtual Task<byte[]> SerializeAsync(object obj)
    {
        return Task.FromResult(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))
        );
    }

    public virtual Task<object> DeserializeAsync(byte[] value, Type type)
    {
        return Task.FromResult(
            JsonConvert.DeserializeObject(Encoding.UTF8.GetString(value), type) ?? throw new InvalidOperationException()
        );
    }

    public virtual Task<object> DeserializeAsync(string stringValue, Type type)
    {
        return Task.FromResult(
            JsonConvert.DeserializeObject(stringValue, type) ?? throw new InvalidOperationException()
        );
    }
}