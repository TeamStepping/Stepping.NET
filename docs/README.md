# Stepping.NET

![build and test](https://img.shields.io/github/workflow/status/TeamStepping/Stepping.NET/Test%20code/main?style=flat-square)
[![codecov](https://codecov.io/gh/TeamStepping/Stepping.NET/branch/main/graph/badge.svg?token=jUKLCxa6HF)](https://codecov.io/gh/TeamStepping/Stepping.NET)
[![NuGet](https://img.shields.io/nuget/v/Stepping.Core.svg?style=flat-square)](https://www.nuget.org/packages/Stepping.Core)
[![NuGet Download](https://img.shields.io/nuget/dt/Stepping.Core.svg?style=flat-square)](https://www.nuget.org/packages/Stepping.Core)

[![WhatCanSteppingDo](https://user-images.githubusercontent.com/30018771/190894723-dd4f1a17-f8f2-4d81-bea1-32f6ab7d4782.png)](https://excalidraw.com/#json=sSS0SSIWEQ3hLKuEgKQbf,g1ijMIFvKb7L8BuoiQYd0w)

Stepping is a distributed [BASE](https://en.wikipedia.org/wiki/Eventual_consistency) jobs implementation. You can use it as a workflow engine, event outbox/inbox, email/SMS sender, remote invoker, and more.

Stepping will **eventually complete** the steps you require. If the app crashes during the executions, the transaction manager will continue to execute the rest steps after it recovers.

It can also work with a DB transaction. Stepping will **eventually complete** the steps you require after the DB transaction commits. You don't need to worry about the inconsistency problem caused by the app crashes after the transaction commits but before the steps' execution. That is implemented based on DTM's [2-phase messaging](https://en.dtm.pub/practice/msg.html) pattern.

> Stepping also supports the "multi-tenant with multi-DB" scenario, meaning it works no matter how many different databases there are in your app.

## What are `Job` and `Step` in Stepping?

`Job` is a distributed transaction unit, and `Step` is a specific task inside a job.

A job contains one or many steps, and the transaction manager will execute them in order. If step 1 fails, it will be retried until success, and then step 2 starts to execute.

## Examples

The transaction manager will eventually complete the added steps:

```csharp
var job = await distributedJobFactory.CreateJobAsync();

job.AddStep(new RequestBank1TransferOutStep(args)); // step with args
job.AddStep<RequestBank2TransferInStep>(); // step without args

await job.StartAsync();
```

The [Steps document](./Steps.md) shows how to define a step.

If you want to execute the steps after a DB transaction commits and ensure they will eventually be done:

```csharp
var db = serviceProvider.GetRequiredService<MyDbContext>(); // example for EF Core
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
