namespace Artifacts.Infrastructure;

public abstract class BaseCompanyEntity
    : IEntity<Guid>, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public abstract class BaseTenantEntity
    : BaseCompanyEntity, ITenantScoped<Guid>
{
    public Guid BranchId { get; set; }
}

public abstract class BaseEntity : BaseTenantEntity
{
}
