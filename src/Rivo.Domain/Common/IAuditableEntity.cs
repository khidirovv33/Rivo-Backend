namespace Rivo.Domain.Common;

/// <summary>Сущности, ключевые изменения которых должны попадать в Audit Log (Who/What/When/OldValue/NewValue).</summary>
public interface IAuditableEntity
{
    Guid? CreatedByUserId { get; set; }

    Guid? UpdatedByUserId { get; set; }
}
