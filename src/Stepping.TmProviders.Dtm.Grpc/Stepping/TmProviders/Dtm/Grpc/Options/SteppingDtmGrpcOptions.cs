using Stepping.TmProviders.Dtm.Grpc.Extensions;

namespace Stepping.TmProviders.Dtm.Grpc.Options;

public class SteppingDtmGrpcOptions
{
    private string _appGrpcUrl = null!;
    private string _executeStepPath = "/stepping.SteppingService/ExecuteStep";
    private string _queryPreparedPath = "/stepping.SteppingService/QueryPrepared";

    /// <summary>
    /// Invoke action APIs with this token for authorization.
    /// </summary>
    public string? ActionApiToken { get; set; }

    public string AppGrpcUrl
    {
        get => _appGrpcUrl;
        set => _appGrpcUrl = value.RemovePreFix("http://").RemovePreFix("https://").RemovePostFix("/");
    }

    /// <summary>
    /// DTM Server gRPC address.
    /// </summary>
    public string DtmGrpcUrl { get; set; } = null!;
    
    /// <summary>
    /// Timeout in milliseconds for DTM Server request. 10,000 (10s) by default.
    /// </summary>
    public int DtmServerRequestTimeout { get; set; } = 10 * 1000;

    /// <summary>
    /// Timeout in milliseconds for branch request. 10,000 (10s) by default.
    /// </summary>
    public int BranchRequestTimeout { get; set; } = 10 * 1000;

    public string ExecuteStepPath
    {
        get => _executeStepPath;
        set => _executeStepPath = value.EnsureStartsWith('/');
    }

    public string QueryPreparedPath
    {
        get => _queryPreparedPath;
        set => _queryPreparedPath = value.EnsureStartsWith('/');
    }

    public string GetExecuteStepAddress()
    {
        return $"{AppGrpcUrl}{ExecuteStepPath}";
    }

    public string GetQueryPreparedAddress()
    {
        return $"{AppGrpcUrl}{QueryPreparedPath}";
    }
}