namespace Stepping.Core.Exceptions;

public class DuplicateBarrierException : SteppingException
{
    public DuplicateBarrierException() : base("Duplicate barrier insert.")
    {
    }
}