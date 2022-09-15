using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Stepping.Core.Jobs;
using Stepping.Core.Steps;
using Stepping.TmProviders.LocalTm.EfCore;
using Stepping.TmProviders.LocalTm.TestApp;

var builder = WebApplication.CreateBuilder(args);

var conn = CreateDatabaseAndGetConnection();
builder.Services.AddStepping(options => { options.RegisterSteps(typeof(PrintToConsoleStep)); });
builder.Services.AddSteppingLocalTm();
builder.Services.AddSteppingLocalTmEfCore(b => { b.UseSqlite(conn); });
builder.Services.AddSteppingLocalTmHostedServiceProcessor();

var app = builder.Build();

Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    using (var scope = app.Services.CreateScope())
    {
        var distributedJobFactory = scope.ServiceProvider.GetRequiredService<IDistributedJobFactory>();

        var job = await distributedJobFactory.CreateJobAsync();

        job.AddStep<PrintToConsoleStep>(); // ExecutableStep + gRPC endpoint
        job.AddStep(new HttpRequestStep(new HttpRequestStepArgs(
                "http://localhost:5236/step1",
                HttpMethod.Post,
                new Dictionary<string, object> { { "hello", "world" } })) // HttpRequestStep + POST
        );
        job.AddStep(new HttpRequestStep(new HttpRequestStepArgs(
                "http://localhost:5236/step2?hello=world",
                HttpMethod.Get)) // HttpRequestStep + GET
        );

        await job.StartAsync();
    }
});

app.MapPost("/step1", async context =>
{
    var input = await context.Request.ReadFromJsonAsync<IDictionary<string, string>>();
    if (input is null || !input.ContainsKey("hello") || input["hello"] != "world")
    {
        Console.WriteLine("Step 1 failed to execute since the form data `hello` was not found.");
        context.Response.StatusCode = 500;
    }

    Console.WriteLine("Step 1 executed.");
    context.Response.StatusCode = 200;
});

app.MapGet("/step2", context =>
{
    context.Response.OnCompleted(async () => { await app.StopAsync(); });

    if (!context.Request.Query.ContainsKey("hello") || context.Request.Query["hello"] != "world")
    {
        Console.WriteLine("Step 2 failed to execute since the query data `hello` was not found.");
        context.Response.StatusCode = 500;
        return Task.CompletedTask;
    }

    Console.WriteLine("Step 2 executed.");
    context.Response.StatusCode = 200;
    return Task.CompletedTask;
});

app.Map("/", () => "Hello world");

app.Run();

SqliteConnection CreateDatabaseAndGetConnection()
{
    var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();

    var options = new DbContextOptionsBuilder<LocalTmDbContext>()
        .UseSqlite(connection)
        .Options;

    using var context = new LocalTmDbContext(options);

    context.GetService<IRelationalDatabaseCreator>().CreateTables();

    return connection;
}