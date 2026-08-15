namespace Rivo.Domain.Exceptions;

/// <summary>Попытка обратиться к сущности, принадлежащей другому tenant — нарушение изоляции данных.</summary>
public class TenantMismatchException : Exception
{
    public TenantMismatchException()
        : base("The requested resource does not belong to the current tenant.")
    {
    }
}
