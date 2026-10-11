using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Commons;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public LookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<LookupItem>> BranchLookupAsync()
    {
        return await _unitOfWork.Lookups.BranchLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> JobLookupAsync()
    {
        return await _unitOfWork.Lookups.JobLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> SalaryTypeLookupAsync()
    {
        return await _unitOfWork.Lookups.SalaryTypeLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> KitchenLookupAsync()
    {
        return await _unitOfWork.Lookups.KitchenLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> MenuSectionLookupAsync()
    {
        return await _unitOfWork.Lookups.MenuSectionLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> StoreLookupAsync()
    {
        return await _unitOfWork.Lookups.StoreLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> ExpenseLookupAsync()
    {
        return await _unitOfWork.Lookups.ExpenseLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> EmployeeLookupAsync()
    {
        return await _unitOfWork.Lookups.EmployeeLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        return await _unitOfWork.Lookups.StorekeeperLookupAsync();
    }

    public async Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId)
    {
        return await _unitOfWork.Lookups.StoreProductLookupByStoreIdAsync(storeId);
    }

    public async Task<StoreProductLookupItem> StoreProductLookupByIdAsync(int id)
    {
        return await _unitOfWork.Lookups.StoreProductLookupByIdAsync(id);
    }
}
