## Use Local-TM As TM Provider

Stepping provides a simple built-in TM implementation. It runs with your app as a local transaction manager. Which app starts a job should be the TM of this job.

### Install in Your Project

1. Install the NuGet package:

   ```powershell
   # Install the main package
   Install-Package Stepping.TmProviders.LocalTm
   
   # Install one of the persistence implementation packages
   Install-Package Stepping.TmProviders.LocalTm.EfCore
   Install-Package Stepping.TmProviders.LocalTm.MongoDb
   ```

   Local-TM use `HostedService` to implement background tasks by default. If you want to implement background tasks yourself, you use `Stepping.TmProviders.LocalTm.Core` instead of `Stepping.TmProviders.LocalTm`

   ```powershell
   Install-Package Stepping.TmProviders.LocalTm.Core
   ```

2. Configure services:

   ```csharp
   // Configure core service
   services.AddSteppingLocalTm();

   // Configure store serivce
   services.AddSteppingLocalTmEfCore(options => 
   { 
      options.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(Program).Assembly.FullName));
   });
   services.AddSteppingLocalTmMongoDb(options =>
   {
      options.ConnectionString = connectionString;
      options.DatabaseName = database;
   });

   // Configure hosted service
   services.AddSteppingLocalTmHostedServiceProcessor();
   ```

   > If you are using the EfCore persistence implementation, you should execute the EfCore migration command before you run it for the first time.

   ```chsarp
   dotnet ef migrations add AddedLocalTm --context LocalTmDbContext
   dotnet ef database update
   ```

   `HostedServiceLocalTmProcessor` is only available for standalone environments by default. If you are using it in a clustered environment, you need to configure the distributed lock provider.  

   Local-TM uses [DistributedLock](https://github.com/madelson/DistributedLock) as a distributed lock implementation. It implements multiple distributed lock providers, and you can choose one for Local-TM `HostedServiceProcessor`. See its [own documentation](https://github.com/madelson/DistributedLock) for details.

   ```csharp
   // Configure redis distributed lock provider for HostedServiceProcessor
   services.AddSteppingLocalTmHostedServiceProcessor(new RedisDistributedSynchronizationProvider(database));
   ```
