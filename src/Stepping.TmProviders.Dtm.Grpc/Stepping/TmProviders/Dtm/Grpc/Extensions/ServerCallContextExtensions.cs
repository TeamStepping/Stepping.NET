using Grpc.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.TmProviders.Dtm.Grpc.Extensions;

public static class ServerCallContextExtensions
{
    public static SteppingDbContextLookupInfoModel CreateDbContextLookupInfoModel(this ServerCallContext context)
    {
        return new SteppingDbContextLookupInfoModel(
            context.GetHeader(DtmRequestHeaderNames.DbProviderName),
            context.GetHeader(DtmRequestHeaderNames.HashedConnectionString),
            context.FindHeader(DtmRequestHeaderNames.DbContextType),
            context.FindHeader(DtmRequestHeaderNames.Database),
            context.FindHeader(DtmRequestHeaderNames.TenantId),
            context.FindHeader(DtmRequestHeaderNames.CustomInfo));
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