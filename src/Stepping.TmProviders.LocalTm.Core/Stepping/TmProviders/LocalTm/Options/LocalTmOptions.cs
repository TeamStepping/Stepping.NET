namespace Stepping.TmProviders.LocalTm.Options;

public class LocalTmOptions
{
    /// <summary>
    /// TM timeout.
    /// Default Value: 60 seconds
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
