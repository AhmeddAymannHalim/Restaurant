using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResturantPL.Helper;

/// <summary>Drop-down and label text of enums from the language files (key Enum.&lt;EnumName&gt;.&lt;Value&gt;), falling back to the value name.</summary>
public static class EnumLocalizationExtensions
{
    public static IEnumerable<SelectListItem> GetLocalizedEnumSelectList<TEnum>(this IHtmlHelper html, IViewLocalizer localizer) where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Select(value => new SelectListItem
        {
            Value = Convert.ToInt32(value).ToString(),
            Text = EnumText(html, localizer, value)
        });
    }

    public static string EnumText(this IHtmlHelper html, IViewLocalizer localizer, Enum value)
    {
        var text = localizer["Enum." + value.GetType().Name + "." + value];

        return text.IsResourceNotFound ? value.ToString() : text.Value;
    }
}
