using Grpc.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.TmProviders.Dtm.Grpc.Extensions;

public static class ServerCallContextExtensions
{
    public static SteppingDbContextInfoModel CreateDbContextInfoModel(this ServerCallContext context)
    {
        return new SteppingDbContextInfoModel(
            context.GetHeader(DtmRequestHeaderNames.DbProviderName),
            context.GetHeader(DtmRequestHeaderNames.DbContextType),
            context.FindHeader(DtmRequestHeaderNames.Database),
            context.GetHeader(DtmRequestHeaderNames.EncryptedConnectionString));
    }
    
    public static string GetHeader(this ServerCallContext context, string headerName)
    {
        return context.FindHeader(headerName) ??
               throw new SteppingException($"Cannot get {headerName} from the gRPC request headers.");
    }
    
    public static string? FindHeader(this ServerCallContext context, string headerName)
    {
        return context.RequestHeaders.GetValue(headerName);
    }
}