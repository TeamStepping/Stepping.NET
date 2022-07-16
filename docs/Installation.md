## Installation

Minimum Stepping consists of the core package and a TM provider.

### Install the Core Package

1. Install the NuGet package:
   ```shell
   Install-Package Stepping.Core
   ```
2. Configure services:
   ```csharp
   services.AddStepping(options => {
       options.RegisterSteps(typeof(MyClass).Assembly); // if you have custom steps
   });
   ```

### Install a TM Provider

Stepping requires transaction managers. You can choose an implementation you like.

* [Use DTM](./Dtm.md)
* [Use local-TM](./LocalTm.md)

### Install DB Providers (optional)

You should install a DB provider if you want to execute the steps after a DB transaction commits.

1. Install the NuGet package:
   ```shell
   Install-Package Stepping.DbProviders.EfCore
   Install-Package Stepping.DbProviders.MongoDb
   ```

2. Configure services:
   ```csharp
   services.AddSteppingEfCore();
   services.AddSteppingMongoDb();
   ```