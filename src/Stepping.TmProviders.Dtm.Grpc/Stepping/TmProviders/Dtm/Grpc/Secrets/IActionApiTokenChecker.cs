namespace Stepping.TmProviders.Dtm.Grpc.Secrets;

public interface IActionApiTokenChecker
{
    Task<bool> IsCorrectAsync(string? token);
}