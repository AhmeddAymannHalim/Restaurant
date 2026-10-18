using Microsoft.AspNetCore.Identity;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Repositories.Definitions;

public class ShiftPasswordRepository : IShiftPasswordRepository
{
    private readonly PasswordHasher<ApplicationSetting> _hasher = new PasswordHasher<ApplicationSetting>();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null, password);
    }

    public bool Verify(string stored, string password, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var isHashed = stored.StartsWith("AQAAAA", StringComparison.Ordinal);
        if (!isHashed)
        {
            needsRehash = stored == password;
            return needsRehash;
        }

        var result = _hasher.VerifyHashedPassword(null, stored, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;

        return result != PasswordVerificationResult.Failed;
    }
}
