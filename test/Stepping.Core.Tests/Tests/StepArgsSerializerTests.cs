using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Steps;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class StepArgsSerializerTests : SteppingCoreTestBase
{
    class MyArgs
    {
        public int Num { get; set; }
        public string Str { get; set; }
        public DateTime Time { get; set; }

        public MyArgs(int num, string str, DateTime time)
        {
            Num = num;
            Str = str;
            Time = time;
        }
    }

    protected static readonly object Args = new MyArgs(1, "text", DateTime.Today);

    protected IStepArgsSerializer StepArgsSerializer { get; }

    public StepArgsSerializerTests()
    {
        StepArgsSerializer = ServiceProvider.GetRequiredService<IStepArgsSerializer>();
    }

    [Fact]
    public async Task Should_Serialize_And_Deserialize_Args()
    {
        var bytes = await StepArgsSerializer.SerializeAsync(Args);
        
        bytes.ShouldNotBeEmpty();
        
        var args = await StepArgsSerializer.DeserializeAsync(bytes, typeof(MyArgs));
        
        args.ShouldBeEquivalentTo(Args);
    }
}