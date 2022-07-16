## Use Self-TM As TM Provider

Stepping provides a built-in self-TM implementation. It runs with your app as a decentralization transaction manager. In short, which app starts a job should be the TM of this job.

### Install in Your Project

1. Install the NuGet package:
   ```shell
   # Install the main package
   Install-Package Stepping.TmProviders.SelfTm
   
   # Install one of the persistence implementation packages
   Install-Package Stepping.TmProviders.SelfTm.EfCore
   Install-Package Stepping.TmProviders.SelfTm.MongoDb
   Install-Package Stepping.TmProviders.SelfTm.Redis
   ```
2. Add services and configure:
   ```csharp
   services.AddSteppingSelfTm();
   
   services.AddSteppingSelfEfCore({todo});
   services.AddSteppingSelfMongoDb({todo});
   services.AddSteppingSelfRedis({todo});
   ```