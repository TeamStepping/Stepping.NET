namespace Stepping.Core;

public class DuplicateBarrierException : SteppingException
{
    public DuplicateBarrierException() : base("Duplicate barrier insert.")
    {
    }
}