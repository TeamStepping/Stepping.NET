namespace Stepping.Core.Infrastructures;

public interface ISteppingJsonSerializer
{
    string Serialize(object obj);

    T Deserialize<T>(string jsonString);

    object Deserialize(Type type, string jsonString);
}