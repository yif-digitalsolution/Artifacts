
namespace Artifacts.Infrastructure;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
    bool IsDeleted { get; set; }
}
