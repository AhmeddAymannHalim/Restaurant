using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IChefRepository
{
    Task<PaginatedResult<ChefItem>> GetPagedChefsAsync(PageRequest request);
    Task<ChefItem> GetChefByIdAsync(int id);
    Task<ResultMessage> UpsertChefAsync(ChefItem entity, IFormFile imageFile);
    Task<ResultMessage> DeleteChefAsync(int id);
}
