namespace Artifacts.EntityFramework;

public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }
    int CreatedBy { get; set; }
    DateTime LastModifiedDate { get; set; }
    int LastModifiedBy { get; set; }
    DateTime? DeletedDate { get; set; }
    int? DeletedBy { get; set; }
}
