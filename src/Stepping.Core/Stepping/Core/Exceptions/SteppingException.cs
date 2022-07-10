namespace Stepping.Core.Exceptions;

public class SteppingException : Exception
{
    public SteppingException()
    {
    }

    public SteppingException(string message)
        : base(message)
    {
    }

    public SteppingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}