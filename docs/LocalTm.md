## Use Local-TM As TM Provider

Stepping provides a simple built-in TM implementation. It runs with your app as a local transaction manager. Which app starts a job should be the TM of this job.

### Install in Your Project

1. Install the NuGet package:
   ```shell
   # Install the main package
   Install-Package Stepping.TmProviders.LocalTm
   
   # Install one of the persistence implementation packages
   Install-Package Stepping.TmProviders.LocalTm.EfCore
   Install-Package Stepping.TmProviders.LocalTm.MongoDb
   Install-Package Stepping.TmProviders.LocalTm.Redis
   ```
2. Add services and configure:
   ```csharp
   services.AddSteppingLocalTm();
   
   services.AddSteppingLocalTmEfCore({todo});
   services.AddSteppingLocalTmMongoDb({todo});
   services.AddSteppingLocalTmRedis({todo});
   ```