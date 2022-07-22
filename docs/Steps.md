## Steps

Before creating a distributed job, you should make some Step classes to determine things to do.

### Executable Step

A sample step class inherits `ExecutableStep<TArgs>`:

```csharp
[StepName("CreateOrder")] // this attribute is not required
public class CreateOrderStep : ExecutableStep<CreateOrderArgs>
{
    public override async Task ExecuteAsync(StepExecutionContext context)
    {
        var db = context.ServiceProvider.GetRequiredService<MyDbContext>();

        if (await db.Orders.AnyAsync(x => x.Gid == context.Gid))
        {
            return; // for idempotent, return if it has been done
        }

        db.Orders.Add(new Order(Args, context.Gid)); // records unique gid for idempotent

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
```

Also, you can make a step class inherits `ExecutableStep` if it doesn't need args:

```csharp
public class SendOrderCreatedEmailStep : ExecutableStep
{
    public override async Task ExecuteAsync(StepExecutionContext context)
    {
        var sender = context.ServiceProvider.GetRequiredService<IOrderEmailSender>();

        await sender.SendForCreatedAsync(context.Gid); // should get order by gid
    }
}
```

An executable step allows running custom code. If you use DTM Server, Stepping will provide a specific API to execute the step; If you use local-TM, it will execute the step locally.

### HTTP Request Step

A sample step to get GitHub organization: (HTTP GET)

```csharp
public class RequestGitHubGetOrganizationStep : HttpRequestStep
{
    public RequestGitHubGetOrganizationStep(string orgName) : base(
        new HttpRequestStepArgs($"https://api.github.com/orgs/{orgName}", HttpMethod.Get))
    {
    }
}
```

A sample step to render GitHub markdown: (HTTP POST)

```csharp
public class RequestGitHubRenderMarkdownStep : HttpRequestStep
{
    public RequestGitHubRenderMarkdownStep(string text) : base(
        new HttpRequestStepArgs(
            "https://api.github.com/markdown",
            HttpMethod.Post,
            new Dictionary<string, object> { { "text", text } }
        ))
    {
    }
}
```

If a step is for HTTP request, `HttpRequestStep` is better than `ExecutableStep` since the former is optimized. For example, if you use DTM Server, it will directly invoke the target endpoint instead of invoking your app (and asking the latter invokes the target endpoint).

### gRPC Request Step

Todo.