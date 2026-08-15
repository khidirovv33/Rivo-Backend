namespace Rivo.Domain.Common;

/// <summary>Изоляция данных между компаниями (tenant) — обязателен для всех сущностей, принадлежащих компании.</summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
