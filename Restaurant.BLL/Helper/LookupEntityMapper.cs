using System.Globalization;
using System.Reflection;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Helper;

/// <summary>Moves a lookup row between its entity and the generic <see cref="LookupEditViewModel"/>.</summary>
public static class LookupEntityMapper
{
    public static LookupEditViewModel ToViewModel<T>(T entity, IEnumerable<LookupField> fields) where T : BaseEntity
    {
        var model = new LookupEditViewModel
        {
            Id = entity.Id,
            Code = Read(entity, "Code") as string,
            ArName = Read(entity, "ArName") as string,
            EnName = Read(entity, "EnName") as string
        };

        foreach (var field in fields)
        {
            model.Extras[field.Name] = Format(Read(entity, field.Name), field.Kind);
        }

        return model;
    }

    public static T ToEntity<T>(LookupEditViewModel model, IEnumerable<LookupField> fields) where T : BaseEntity, new()
    {
        var entity = new T { Id = model.Id };

        Write(entity, "Code", string.IsNullOrWhiteSpace(model.Code) ? null : model.Code.Trim());
        Write(entity, "ArName", model.ArName?.Trim());
        Write(entity, "EnName", model.EnName?.Trim());

        foreach (var field in fields)
        {
            model.Extras.TryGetValue(field.Name, out var raw);
            Write(entity, field.Name, Parse(raw, field, typeof(T).GetProperty(field.Name)?.PropertyType));
        }

        return entity;
    }

    private static object Read(object entity, string name)
    {
        return entity.GetType().GetProperty(name)?.GetValue(entity);
    }

    private static void Write(object entity, string name, object value)
    {
        var property = entity.GetType().GetProperty(name);
        if (property is null || !property.CanWrite)
        {
            return;
        }

        if (value is null && Nullable.GetUnderlyingType(property.PropertyType) is null && property.PropertyType.IsValueType)
        {
            return;
        }

        property.SetValue(entity, value);
    }

    private static string Format(object value, LookupFieldKind kind)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return kind switch
        {
            LookupFieldKind.Bool => ((bool)value) ? "true" : "false",
            LookupFieldKind.Enum => Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture),
            LookupFieldKind.Decimal => Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    private static object Parse(string raw, LookupField field, Type propertyType)
    {
        if (propertyType is null)
        {
            return null;
        }

        var target = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var blank = string.IsNullOrWhiteSpace(raw);

        switch (field.Kind)
        {
            case LookupFieldKind.Text:
                return blank ? null : raw.Trim();

            case LookupFieldKind.Bool:
                // The checkbox posts "true" and a hidden "false" after it; either way "true" wins when ticked.
                return !blank && raw.Split(',').Any(a => a.Trim().Equals("true", StringComparison.OrdinalIgnoreCase));

            case LookupFieldKind.Decimal:
                return !blank && decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var number)
                    ? Convert.ChangeType(number, target, CultureInfo.InvariantCulture)
                    : null;

            case LookupFieldKind.Int:
            case LookupFieldKind.Parent:
            case LookupFieldKind.Reference:
                return !blank && int.TryParse(raw.Split(',')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var whole) && (field.Kind == LookupFieldKind.Int || whole > 0)
                    ? Convert.ChangeType(whole, target, CultureInfo.InvariantCulture)
                    : null;

            case LookupFieldKind.Enum:
                return !blank && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                    ? Enum.ToObject(target, value)
                    : null;

            default:
                return null;
        }
    }
}
