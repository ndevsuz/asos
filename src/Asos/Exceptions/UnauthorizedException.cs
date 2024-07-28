namespace Asos.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    { }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    { }

    public int StatusCode { get; set; } = 404;
}