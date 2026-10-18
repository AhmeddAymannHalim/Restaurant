using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAccountCodeRepository
{
    List<DefaultAccount> GetDefaultAccounts();
    bool IsSystemCode(string code);
}
