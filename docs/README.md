![logo-white](https://user-images.githubusercontent.com/30018771/178152345-49f6e952-d8f9-4999-96ac-682ff81641e0.png)

# Stepping.NET
Stepping is a distributed [BASE](https://en.wikipedia.org/wiki/Eventual_consistency) jobs implementation. You can use it as a workflow engine, event outbox/inbox, email/SMS sender, remote invoker, and more. 

It can also work with a DB transaction and supports the multi-DB scenario. That means the jobs are **ensured to be eventually done** after the DB transactions commit. You don't need to worry about inconsistencies caused by the app crashes after the transaction commit but before the steps are executed.

The distributed transaction is based on DTM's [2-phase messaging](https://en.dtm.pub/practice/msg.html) pattern.

## What are `Job` and `Step` in Stepping?

`Job` is a distributed transaction unit, and `Step` is a specific task inside a job.

A job contains some steps, and the TM will execute them in order. If step 1 fails, it will be retried until success, and then step 2 starts to execute.

## Examples (for EF Core)

The TM will eventually complete the added steps:

```csharp
var job = await distributedJobFactory.CreateJobAsync();

job.AddStep(new CreateOrderStep(orderCreatingArgs));
job.AddStep<SendOrderCreatedEmailStep>();

await job.StartAsync();
```

The [Steps document](./Steps.md) shows how to define a step.

If you want to execute the steps after a DB transaction commits and ensure they will eventually be done:

```csharp
var db = serviceProvider.GetRequiredService<MyDbContext>();
await db.Database.BeginTransactionAsync();

var order = new Order(args);

db.Orders.Add(order);
await db.SaveChangesAsync();

var job = await distributedJobFactory.CreateJobAsync(new EfCoreSteppingDbContext(db));

job.AddStep(new SendOrderCreatedEmailStep(order));
job.AddStep(new SendOrderCreatedSmsStep(order));

await job.StartAsync(); // it will commit the DB transaction
```

Stepping supports `EF Core`, `ADO.NET`(coming soon), and `MongoDB`.

For details, please see the [Usage document](./Usage.md).

## Installation

See the [Installation document](./Installation.md).

## Supported Transaction Managers

Stepping requires transaction managers. You can choose an implementation you like.

### DTM Server

DTM is a mature transaction manager you can use as the TM provider for Stepping. DTM allows you to get many other distributed transaction modes like SAGA, TCC, and XA.

See the [DTM document](./Dtm.md).

### Local-TM

Stepping provides a simple built-in TM implementation. It runs with your app as a local transaction manager. Which app starts a job should be the TM of this job.

See the [Local-TM document](./LocalTm.md).
