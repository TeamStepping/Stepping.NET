namespace Stepping.TestBase.Fakes;

public class FakeArgs
{
    public string Name { get; set; } = null!;

    protected FakeArgs()
    {
    }

    public FakeArgs(string name)
    {
        Name = name;
    }
}