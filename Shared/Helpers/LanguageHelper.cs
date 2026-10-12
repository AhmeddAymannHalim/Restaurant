using System.Globalization;

public static class LanguageHelper
{
    public static bool IsArabic()
    {
        bool isArabicCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";
        return isArabicCulture;
    }

    public static string NameColumn()
    {
        return IsArabic() ? "ArName" : "EnName";
    }

    public static string LanguageName()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? "A" : "E";
    }
}
