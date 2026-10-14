using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResturantPL.Helper;

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

    public static IEnumerable<SelectListItem> GetLocalizedEnumNameSelectList<TEnum>(this IHtmlHelper html, IViewLocalizer localizer, string selected) where TEnum : struct, Enum
    {
        var items = Enum.GetValues<TEnum>().Select(value => new SelectListItem
        {
            Value = value.ToString(),
            Text = EnumText(html, localizer, value),
            Selected = string.Equals(value.ToString(), selected, StringComparison.OrdinalIgnoreCase)
        }).ToList();

        if (!string.IsNullOrWhiteSpace(selected) && !items.Any(i => i.Selected))
        {
            items.Add(new SelectListItem { Value = selected, Text = selected, Selected = true });
        }

        return items;
    }

    public static string EnumText(this IHtmlHelper html, IViewLocalizer localizer, Enum value)
    {
        var text = localizer["Enum." + value.GetType().Name + "." + value];

        return text.IsResourceNotFound ? value.ToString() : text.Value;
    }
}
