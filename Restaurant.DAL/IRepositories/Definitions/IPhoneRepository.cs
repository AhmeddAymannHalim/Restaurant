using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IPhoneRepository
{
    List<string> CleanNumbers(string mainNumber, IEnumerable<IPhoneRow> extraNumbers);
    void SyncCustomerPhones(ICollection<CustomerPhone> current, List<string> wanted);
    void SyncStorePhones(ICollection<StorePhone> current, List<string> wanted);
    void SyncSupplierPhones(ICollection<SupplierPhone> current, List<string> wanted);
}
