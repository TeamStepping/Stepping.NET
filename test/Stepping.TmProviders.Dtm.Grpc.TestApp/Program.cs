using Stepping.Core.Jobs;
using Stepping.Core.Steps;
using Stepping.TmProviders.Dtm.Grpc.Services;
using Stepping.TmProviders.Dtm.Grpc.TestApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddStepping(options => { options.RegisterSteps(typeof(PrintToConsoleStep)); });
builder.Services.AddSteppingDtmGrpc(options =>
{
    options.ActionApiToken =
        "KLyqz0VS3mOc6VY1"; // DTM Server invokes app's action APIs with this token for authorization.
    options.AppGrpcUrl =
        "http://localhost:5235"; // Base URL for DTM Server to invoke the current app. Only HTTP scheme now!
    options.DtmGrpcUrl = "http://localhost:36790"; // Base URL for the current app to invoke DTM Server.
});

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(b => b.MapGrpcService<SteppingService>());

var distributedJobFactory = app.Services.GetRequiredService<IDistributedJobFactory>();

var job = await distributedJobFactory.CreateJobAsync();

job.AddStep<PrintToConsoleStep>(); // ExecutableStep + gRPC endpoint
job.AddStep(new HttpRequestStep(new HttpRequestStepArgs(
    "http://localhost:5235/step1",
    HttpMethod.Post,
    new Dictionary<string, object> { { "hello", "world" } })) // HttpRequestStep + POST + payload
);
job.AddStep(new HttpRequestStep(new HttpRequestStepArgs(
    "http://localhost:5235/step2",
    HttpMethod.Get,
    new Dictionary<string, object> { { "hello", "world" } })) // HttpRequestStep + GET + payload
);

await job.StartAsync();

app.MapPost("/step1", context =>
{
    if (context.Request.Form.ContainsKey("hello") || context.Request.Form["hello"] != "world")
    {
        Console.WriteLine("Step 1 failed to execute since the form data `hello` was not found.");
        context.Response.StatusCode = 500;
        return Task.CompletedTask;
    }

    Console.WriteLine("Step 1 executed.");
    context.Response.StatusCode = 200;
    return Task.CompletedTask;
});

app.MapGet("/step2", context =>
{
    if (context.Request.Query.ContainsKey("hello") || context.Request.Query["hello"] != "world")
    {
        Console.WriteLine("Step 2 failed to execute since the query data `hello` was not found.");
        context.Response.StatusCode = 500;
        return Task.CompletedTask;
    }

    Console.WriteLine("Step 2 executed.");
    context.Response.StatusCode = 200;
    return Task.CompletedTask;
});

app.Run();