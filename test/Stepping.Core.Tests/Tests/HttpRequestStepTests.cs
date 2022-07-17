using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Infrastructures;
using Stepping.Core.Steps;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class HttpRequestStepTests : SteppingCoreTestBase
{
    protected ISteppingJsonSerializer JsonSerializer { get; }

    public HttpRequestStepTests()
    {
        JsonSerializer = ServiceProvider.GetRequiredService<ISteppingJsonSerializer>();
    }

    [Fact]
    public Task Should_Create_HttpRequestMessage_For_Get()
    {
        var step = new HttpRequestStep(new HttpRequestStepArgs("https://fakeurl.com", HttpMethod.Get, null,
            new List<KeyValuePair<string, string>>
            {
                new("my_header1", "1234")
            }));

        var requestMessage = step.CreateHttpRequestMessage(JsonSerializer);

        requestMessage.ShouldNotBeNull();
        requestMessage.Method.ShouldBe(HttpMethod.Get);
        requestMessage.RequestUri.ShouldBeEquivalentTo(new Uri("https://fakeurl.com"));
        requestMessage.Content.ShouldBeNull();
        requestMessage.Headers.ShouldContain(x => x.Key == "my_header1" && x.Value.Contains("1234"));

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_Create_HttpRequestMessage_For_Post()
    {
        var step = new HttpRequestStep(new HttpRequestStepArgs("https://fakeurl.com", HttpMethod.Post,
            new Dictionary<string, object>
            {
                { "username", "John" }
            },
            new List<KeyValuePair<string, string>>
            {
                new("my_header1", "1234")
            }));

        var requestMessage = step.CreateHttpRequestMessage(JsonSerializer);

        requestMessage.ShouldNotBeNull();
        requestMessage.Method.ShouldBe(HttpMethod.Post);
        requestMessage.RequestUri.ShouldBeEquivalentTo(new Uri("https://fakeurl.com"));
        requestMessage.Content.ShouldBeAssignableTo<StringContent>();
        (await ((StringContent)requestMessage.Content!).ReadAsStringAsync()).ShouldBe("{\"username\":\"John\"}");
        requestMessage.Headers.ShouldContain(x => x.Key == "my_header1" && x.Value.Contains("1234"));
    }
}