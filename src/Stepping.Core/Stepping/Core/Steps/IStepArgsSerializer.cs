namespace Stepping.Core.Steps;

public interface IStepArgsSerializer
{
    Task<byte[]> SerializeAsync(object obj);

    Task<object> DeserializeAsync(byte[] value, Type type);

    Task<object> DeserializeAsync(string stringValue, Type type);
}