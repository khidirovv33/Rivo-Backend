namespace Rivo.Application.Common.Interfaces;

/// <summary>Текущий tenant (компания) запроса — резолвится из JWT-claim'а middleware'ом в API-слое.</summary>
public interface ICurrentTenantService
{
    Guid? TenantId { get; }
}
