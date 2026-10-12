namespace Shared.Helpers;

public static class WeekDays
{
    public static readonly string[] Names =
    {
        "Saturday",
        "Sunday",
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday"
    };

    private static readonly Dictionary<string, string> Arabic = new()
    {
        ["السبت"] = "Saturday",
        ["الأحد"] = "Sunday",
        ["الاحد"] = "Sunday",
        ["الاثنين"] = "Monday",
        ["الإثنين"] = "Monday",
        ["الثلاثاء"] = "Tuesday",
        ["الأربعاء"] = "Wednesday",
        ["الاربعاء"] = "Wednesday",
        ["الخميس"] = "Thursday",
        ["الجمعة"] = "Friday"
    };

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        if (Arabic.TryGetValue(text, out var english))
        {
            return english;
        }

        return Names.FirstOrDefault(a => string.Equals(a, text, StringComparison.OrdinalIgnoreCase));
    }
}
