using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IPhoneRepository
{
    List<string> CleanNumbers(string mainNumber, IEnumerable<IPhoneRow> extraNumbers);
    void SyncSupplierPhones(ICollection<SupplierPhone> current, List<string> wanted);
}
