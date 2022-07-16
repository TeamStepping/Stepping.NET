![logo-white](https://user-images.githubusercontent.com/30018771/178152345-49f6e952-d8f9-4999-96ac-682ff81641e0.png)

# Stepping.NET
Stepping is a distributed [BASE](https://en.wikipedia.org/wiki/Eventual_consistency) jobs implementation. You can use it as a workflow engine, event outbox/inbox, email/SMS sender, remote invoker, and more. 

The distributed transaction is based on DTM's [2-phase messaging](https://en.dtm.pub/practice/msg.html) pattern.

## What are `Job` and `Step` in Stepping?

`Job` is a distributed transaction unit, and `Step` is a specific task inside a job. A job contains some steps and executes them in order. If step 1 fails, it will be retried until success, and then step 2 starts to execute.

If a job involves a DB transaction, the steps will be **ensured to be eventually done** after the transaction is committed. You don't need to worry about inconsistencies caused by the app crashes after the transaction commit but before the steps are executed.

## Examples

Define steps:
```csharp
[StepName("CreateOrder")]
public class CreateOrderStep : ExecutableStep<CreateOrderArgs>
{
    public override async Task ExecuteAsync(StepExecutionContext context)
    {
        var orderManager = context.ServiceProvider.GetRequiredService<IOrderManager>();
        await orderManager.CreateOrderAsync(Args);
    }
}

[StepName("SendOrderCreatedEmail")]
public class SendOrderCreatedEmailStep : ExecutableStep
{
    public override async Task ExecuteAsync(StepExecutionContext context)
    {
        var sender = context.ServiceProvider.GetRequiredService<IOrderEmailSender>();
        await sender.SendForCreatedAsync(context.Gid); // should get order by gid
    }
}
```
The added steps will be eventual done by TM:
```csharp
var job = await DistributedJobFactory.CreateJobAsync();

job.AddStep(new CreateOrderStep(orderCreatingArgs));
job.AddStep<SendOrderCreatedEmailStep>();

await job.StartAsync();
```
If you want to execute the steps after a DB transaction commits and want to ensure they will eventually be done:
```csharp
var steppingDbContext = new EfCoreSteppingDbContext(efCoreDbContext);
var job = await DistributedJobFactory.CreateJobAsync(steppingDbContext);
```
For more, please see the [usage document](./Usage.md).

## Supported Transaction Managers

Stepping requires transaction managers. You can choose an implementation you like.

### DTM Server

DTM Server is a mature transaction manager you can use as the TM provider for Stepping. DTM allows you to get many other distributed transaction modes like SAGA, TCC, and XA.

See the [DTM document](./Dtm.md).

### Stepping Self-TM

Stepping provides a built-in self-TM implementation. It runs with your app as a decentralization transaction manager. In short, which app starts a job should be the TM of this job.

See the [Self-TM document](./SelfTm.md).
