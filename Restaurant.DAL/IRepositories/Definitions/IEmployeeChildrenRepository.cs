using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IEmployeeChildrenRepository
{
    void PrepareNew(Employee entity, IList<IFormFile> documentFiles);
    void Sync(Employee existing, Employee incoming, IList<IFormFile> documentFiles);
    void Remove(Employee employee);
}
