using Newtonsoft.Json;

namespace Stepping.Core.Infrastructures;

public class NewtonsoftSteppingJsonSerializer : ISteppingJsonSerializer
{
    public virtual string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public virtual T Deserialize<T>(string jsonString)
    {
        return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new InvalidOperationException();
    }

    public virtual object Deserialize(Type type, string jsonString)
    {
        return JsonConvert.DeserializeObject(jsonString, type) ?? throw new InvalidOperationException();
    }
}