using Microsoft.AspNetCore.Identity;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Helpers;

public static class ShiftPassword
{
    private static readonly PasswordHasher<ApplicationSetting> Hasher = new();

    public static string Hash(string password)
    {
        return Hasher.HashPassword(null, password);
    }

    public static bool Verify(string stored, string password, out bool needsRehash)
    {
        needsRehash = false;
        if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        if (!stored.StartsWith("AQAAAA", StringComparison.Ordinal))
        {
            needsRehash = stored == password;
            return needsRehash;
        }

        var result = Hasher.VerifyHashedPassword(null, stored, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;

        return result != PasswordVerificationResult.Failed;
    }
}
