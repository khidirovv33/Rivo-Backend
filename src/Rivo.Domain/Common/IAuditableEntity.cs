namespace Rivo.Domain.Common;

/// <summary>Marker for entities whose Create/Update/Delete changes are written to the AuditLog by the SaveChanges interceptor.</summary>
public interface IAuditableEntity
{
    Guid? CreatedBy { get; set; }
    Guid? UpdatedBy { get; set; }
}
