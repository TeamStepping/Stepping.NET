## Use DTM Server As TM Provider

DTM Server is a mature transaction manager you can use as the TM provider for Stepping. DTM allows you to get many other distributed transaction modes like SAGA, TCC, and XA.

### Install DTM Server

See DTM's [official document](https://en.dtm.pub/guide/install.html) to learn how to install the DTM Server.

### Install Client in Your Project

1. Install the NuGet package:
   ```shell
   Install-Package Grpc.AspNetCore
   Install-Package Stepping.TmProviders.Dtm.Grpc
   ```
2. Configure services:
   ```csharp
   services.AddGrpc();
   services.AddSteppingDtmGrpc(options =>
   {
       options.ActionApiToken = "KLyqz0VS3mOc6VY1"; // DTM Server invokes app's action APIs with this token for authorization.
       options.AppGrpcUrl = "http://localhost:5000"; // Base URL for DTM Server to invoke the current app. Only HTTP scheme now!
       options.DtmGrpcUrl = "http://localhost:36790"; // Base URL for the current app to invoke DTM Server.
   });
   ```
3. Configure gRPC services:
   ```csharp
   app.UseRouting();
   app.UseEndpoints(builder => builder.MapGrpcService<SteppingService>());
   ```