using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAccountProvisionRepository
{
    Task<Account> CreateAsync(string arName, string enName, AccountType type, string parentCode, decimal openingBalance = 0);
}
