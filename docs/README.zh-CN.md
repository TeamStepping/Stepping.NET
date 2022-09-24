# Stepping.NET

![build and test](https://img.shields.io/github/workflow/status/TeamStepping/Stepping.NET/Test%20code/main?style=flat-square)
[![codecov](https://codecov.io/gh/TeamStepping/Stepping.NET/branch/main/graph/badge.svg?token=jUKLCxa6HF)](https://codecov.io/gh/TeamStepping/Stepping.NET)
[![NuGet](https://img.shields.io/nuget/v/Stepping.Core.svg?style=flat-square)](https://www.nuget.org/packages/Stepping.Core)
[![NuGet Download](https://img.shields.io/nuget/dt/Stepping.Core.svg?style=flat-square)](https://www.nuget.org/packages/Stepping.Core)

Stepping 是一个基于 [BASE](https://en.wikipedia.org/wiki/Eventual_consistency) 的分布式作业实现。它可以作为工作流引擎，事件收/发件箱，用于邮箱/短信发送，用于远程接口调用等场景。

我们已为以下语言提供了文档：[English](./README.md)，[简体中文](./README.zh-CN.md)。

## Stepping 中 `Job` 和 `Step` 是什么?

`Job` 是一个分布式事务单元，而 `Step` 是 job 中一个特定的任务。

一个 job（作业）包含了一个或多个 step（步骤），事务管理器会按顺序执行步骤。如果步骤 1 失败了，它将重试直到成功，然后开始执行步骤 2。

## 什么场景需要 Stepping

[![WhatCanSteppingDo](https://user-images.githubusercontent.com/30018771/190923267-38cae2ff-29de-4219-bd7f-423ff6cb98f5.png)](https://excalidraw.com/#json=5PXRUbpKnk6rBiEz5zebr,_AUzbfwUZM24qqCcBoOsUw)

### 需要执行多个步骤且确保原子性

当一个 job 开始执行，Stepping 最终会完成你布置的所有 steps。如果你的应用在执行这些步骤期间挂了，事务管理器会在应用恢复后，继续执行剩下的步骤。

Stepping 会按顺序挨个完成你布置的 steps。如果一个步骤失败，它会被推迟重试，这确保了 job 的 [原子性](https://coffeecodeclimb.com/2020/07/26/atomicity-and-idempotency-for-dummies/#atomicity)。请确保你所有的 step 都能在重试后最终成功，除非它是一个 [Saga step](./Steps.md#saga-step)。

当你的应用在执行步骤期间挂了，Stepping 有可能已经实际完成了这个步骤，而未自知，当你的应用恢复，Stepping 会冗余地执行这个步骤。因此，你所有的步骤都应该做到 [幂等](https://coffeecodeclimb.com/2020/07/26/atomicity-and-idempotency-for-dummies/#idempotence)。

### 需要确保在 DB 事务提交后，后续步骤一定执行

当一个绑定了 DB 事务的 job 开始执行，在 DB 事务提交后，Stepping 最终会完成你布置的所有 steps。

你无需担心在 DB 事务提交后、后续步骤执行之前，这期间应用挂了导致的非原子性问题。我们已经使用 DTM 的 [二阶段消息](https://en.dtm.pub/practice/msg.html) 模式处理了这种情况。

Stepping 也支持“多租户且多数据库”的场景，这意味着无论你的应用有多少个不同的数据库，都不成问题。

## 用例

事务管理器会最终完成添加的步骤：

```csharp
var job = await distributedJobFactory.CreateJobAsync();

job.AddStep(new RequestBank1TransferOutStep(args)); // 带参数的步骤
job.AddStep<RequestBank2TransferInStep>(); // 不带参数的步骤

await job.StartAsync();
```

[Steps 文档](./Steps.md) 介绍了如何定义一个步骤。

如果你希望在 DB 事务提交后开始执行一些步骤，并且确保它们最终能够执行成功：

```csharp
var db = serviceProvider.GetRequiredService<MyDbContext>(); // 以 EF Core 举例
await db.Database.BeginTransactionAsync();

var order = new Order(args);

db.Orders.Add(order);
await db.SaveChangesAsync();

var job = await distributedJobFactory.CreateJobAsync(new EfCoreSteppingDbContext(db));

job.AddStep(new SendOrderCreatedEmailStep(order));
job.AddStep(new SendOrderCreatedSmsStep(order));

await job.StartAsync(); // 这个方法也会提交 DB 事务
```

Stepping 支持 `EF Core`，`ADO.NET`(即将到来)，及 `MongoDB`。

了解更多信息，请参阅 [用法文档](./Usage.md)。

## 安装

请参阅 [安装文档](./Installation.md)。

## 支持的事务管理器

Stepping 要求使用事务管理器。你可以选择一种你喜欢的事务管理器。

### Local-TM

Stepping 提供了一种简单的内置事务管理器实现。Local-TM 与你的应用一起运行。在这种模式下，每个应用都作为自己发布的 jobs 的事务管理器。

这种事务管理器适用于开发和测试环境，以及并发度较低的应用，不用额外维护组件

请参阅 [Local-TM 文档](./LocalTm.md)。

### DTM Server

DTM 是一个成熟的事务管理器，并且能够为 Stepping 提供能力。选择 DTM ，对于并发度高的应用，性能表现更好。你也可以使用DTM的SDK，支持更多的分布式事务模式，例如 Saga、TCC和XA。

请参阅 [DTM 文档](./Dtm.md)。

