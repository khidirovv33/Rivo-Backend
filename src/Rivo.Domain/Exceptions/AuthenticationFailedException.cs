namespace Rivo.Domain.Exceptions;

/// <summary>Invalid credentials, blocked account, or bad/expired token — mapped to HTTP 401 by the middleware.</summary>
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message) : base(message)
    {
    }
}
