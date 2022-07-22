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

Stepping DB Providers are not required.

If you want to execute the steps after a DB transaction commits and ensure they will eventually be done, you should install them.

1. Install the NuGet package:
   ```shell
   Install-Package Stepping.DbProviders.EfCore
   Install-Package Stepping.DbProviders.MongoDb
   ```

2. Configure services:
   ```csharp
   services.AddSteppingEfCore();
   services.AddSteppingMongoDb(options => {
       // you can implement ISteppingDbContextProvider to customize the connection string lookup.
       options.DefaultConnectionString = "mongodb://root:password123@198.174.21.23:27017"
   });
   ```

Also, see the [DB Provider document](./DbProviders.md) for usage.