namespace Stepping.TmProviders.LocalTm.Timing;

public class SteppingClock : ISteppingClock
{
    public DateTime Now => DateTime.UtcNow;
}
