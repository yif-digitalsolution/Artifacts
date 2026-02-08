

namespace Artifacts.Infrastructure;

public interface ICompany<T> 
{
     int CompanyId { get; set; }
     T Company { get; set; }
}