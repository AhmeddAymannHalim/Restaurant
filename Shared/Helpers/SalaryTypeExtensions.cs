using Shared.Enums;

namespace Shared.Helpers;

public static class SalaryTypeExtensions
{
    public static string ArName(this SalaryType type)
    {
        return type switch
        {
            SalaryType.Monthly => "شهري",
            SalaryType.Weekly => "اسبوعي",
            SalaryType.Daily => "يومي",
            SalaryType.Hourly => "بالساعة",
            _ => type.ToString()
        };
    }

    public static string EnName(this SalaryType type)
    {
        return type.ToString();
    }
}
