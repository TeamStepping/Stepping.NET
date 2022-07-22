## DB Providers

Stepping DB Providers are not required.

If you want to execute the steps after a DB transaction commits and ensure they will eventually be done, you should install them.

Supported providers:
* [EF Core](#ef-core)
* [ADO.NET](#adonet)
* [MongoDB](#mongodb)

### EF Core

The regular usage:

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

Use methods of `IAdvancedDistributedJob`: (it's equivalent to the above)

```csharp
var db = serviceProvider.GetRequiredService<MyDbContext>();
await db.Database.BeginTransactionAsync();

var order = new Order(args);

db.Orders.Add(order);
await db.SaveChangesAsync();

var job = await distributedJobFactory.CreateAdvancedJobAsync(new EfCoreSteppingDbContext(db));

job.AddStep(new SendOrderCreatedEmailStep(order));
job.AddStep(new SendOrderCreatedSmsStep(order));

await job.PrepareAndInsertBarrierAsync();
await db.Database.CommitTransactionAsync(); // manually commit
await job.SubmitAsync();
```

### ADO.NET

Todo.

### MongoDB

The regular usage:

```csharp
var client = new MongoClient(connectionString);
var sessionHandle = await client.StartSessionAsync();
sessionHandle.StartTransaction();

var steppingDbContext = new MongoDbSteppingDbContext(
    client, client.GetDatabase(databaseName), sessionHandle, connectionString);

var order = new Order(args);

var collection = steppingDbContext.Database.GetCollection<Order>("Orders");
await collection.InsertOneAsync(order);

var job = await distributedJobFactory.CreateJobAsync(steppingDbContext);

job.AddStep(new SendOrderCreatedEmailStep(order));
job.AddStep(new SendOrderCreatedSmsStep(order));

await job.StartAsync(); // it will commit the DB transaction
```

Use methods of `IAdvancedDistributedJob`: (it's equivalent to the above)

```csharp
var client = new MongoClient(connectionString);
var sessionHandle = await client.StartSessionAsync();
sessionHandle.StartTransaction();

var steppingDbContext = new MongoDbSteppingDbContext(
    client, client.GetDatabase(databaseName), sessionHandle, connectionString);

var order = new Order(args);

var collection = steppingDbContext.Database.GetCollection<Order>("Orders");
await collection.InsertOneAsync(order);

var job = await distributedJobFactory.CreateJobAsync(steppingDbContext);

job.AddStep(new SendOrderCreatedEmailStep(order));
job.AddStep(new SendOrderCreatedSmsStep(order));

await job.PrepareAndInsertBarrierAsync();
await sessionHandle.CommitTransactionAsync(); // manually commit
await job.SubmitAsync();
```