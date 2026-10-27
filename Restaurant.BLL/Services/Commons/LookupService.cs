using AutoMapper;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Commons;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LookupService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }



    public async Task<IEnumerable<DeductionTypeViewModel>> DeductionTypeLookupAsync()
    {
        return await _unitOfWork.Lookups.DeductionTypeLookupAsync();
    }

    public async Task<IEnumerable<AllowanceTypeViewModel>> AllowanceTypeLookupAsync()
    {
        return await _unitOfWork.Lookups.AllowanceTypeLookupAsync();
    }

    public async Task<IEnumerable<LeaveTypeViewModel>> LeaveTypeLookupAsync()
    {
        return await _unitOfWork.Lookups.LeaveTypeLookupAsync();
    }

    public async Task<IEnumerable<InsuranceCategoryViewModel>> InsuranceCategoryLookupAsync()
    {
        return await _unitOfWork.Lookups.InsuranceCategoryLookupAsync();
    }


    public async Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync()
    {
        return await _unitOfWork.Lookups.DeliveryDriverLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> FreeTableLookupAsync()
    {
        return await _unitOfWork.Lookups.FreeTableLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        return await _unitOfWork.Lookups.StorekeeperLookupAsync();
    }





    public async Task<IEnumerable<DeliveryZoneViewModel>> DeliveryZoneLookupAsync()
    {
        var zones = await _unitOfWork.DeliveryZones.GetAllDeliveryZonesAsync();

        return _mapper.Map<List<DeliveryZoneViewModel>>(zones);
    }





    public async Task<IEnumerable<LookupItem>> SupplierLookupAsync()
    {
        return await _unitOfWork.Lookups.SupplierLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> CustomerLookupAsync()
    {
        return await _unitOfWork.Lookups.CustomerLookupAsync();
    }

    public async Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId)
    {
        return await _unitOfWork.Lookups.StoreProductLookupByStoreIdAsync(storeId);
    }


    public async Task<IEnumerable<LookupItem_String>> RoleLookupAsync()
    {
        return await _unitOfWork.Lookups.RoleLookupAsync();
    }


    public async Task<IEnumerable<LookupItem>> GovernorateLookupAsync(int countryId)
    {
        return await _unitOfWork.Lookups.GovernorateLookupAsync(countryId);
    }

    public async Task<IEnumerable<LookupItem>> CityLookupAsync(int governorateId)
    {
        return await _unitOfWork.Lookups.CityLookupAsync(governorateId);
    }

    public async Task<IEnumerable<LookupItem>> NamedLookupAsync<T>() where T : class
    {
        return await _unitOfWork.Lookups.NamedLookupAsync<T>();
    }

    public async Task<IEnumerable<LookupItem>> VillageLookupAsync(int townId)
    {
        return await _unitOfWork.Lookups.VillageLookupAsync(townId);
    }
}
