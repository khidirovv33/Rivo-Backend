namespace Rivo.Domain.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Access to this resource is forbidden.")
        : base(message)
    {
    }
}
