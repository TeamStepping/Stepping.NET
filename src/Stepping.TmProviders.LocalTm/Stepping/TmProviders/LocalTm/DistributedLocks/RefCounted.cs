namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public class RefCounted<T>
{
    public string Resource { get; }

    public int RefCount { get; private set; }

    public T Value { get; }

    public RefCounted(string resource, T value)
    {
        Resource = resource;
        RefCount = 1;
        Value = value;
    }

    public void Increase()
    {
        RefCount++;
    }

    public void Decrease()
    {
        RefCount--;
    }
}