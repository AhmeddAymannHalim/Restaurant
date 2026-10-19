using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAccountProvisionRepository
{
    Task<Account> CreateAsync(string arName, string enName, AccountType type, decimal openingBalance = 0);
}
