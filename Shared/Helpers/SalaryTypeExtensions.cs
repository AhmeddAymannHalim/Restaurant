using Shared.Enums;

namespace Shared.Helpers;

public static class SalaryTypeExtensions
{
    public static string ArName(this SalaryType type)
    {
        return type switch
        {
            SalaryType.Cash => "كاش",
            SalaryType.Visa => "فيزا",
            _ => type.ToString()
        };
    }

    public static string EnName(this SalaryType type)
    {
        return type.ToString();
    }
}
