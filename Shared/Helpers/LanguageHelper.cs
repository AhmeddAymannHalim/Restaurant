using System.Globalization;

public static class LanguageHelper
{
    public static bool IsArabic()
    {
        bool isArabicCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";
        return isArabicCulture;
    }
}
