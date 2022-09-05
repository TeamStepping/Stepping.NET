namespace Stepping.TmProviders.LocalTm.Timing;

public interface ISteppingClock
{
    DateTime Now { get; }
}