namespace Artifacts.Infrastructure;

public interface ITenantScoped<TKey>
{
    TKey CompanyId { get; set; }
    TKey BranchId { get; set; }
}

