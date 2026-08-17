namespace Rivo.Domain.Exceptions;

public class TenantMismatchException : Exception
{
    public TenantMismatchException()
        : base("The requested resource does not belong to the current tenant.")
    {
    }
}
