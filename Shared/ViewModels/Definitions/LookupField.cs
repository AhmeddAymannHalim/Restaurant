using Shared.Enums;

namespace Shared.ViewModels.Definitions;

/// <summary>Describes one extra column of a simple lookup table so a single generic screen can edit it.</summary>
public class LookupField
{
    public LookupField(string name, LookupFieldKind kind, Type enumType = null)
    {
        Name = name;
        Kind = kind;
        EnumType = enumType;
    }

    /// <summary>The property name on the entity (also the key in the form and the resource key suffix).</summary>
    public string Name { get; }

    public LookupFieldKind Kind { get; }

    /// <summary>Only for <see cref="LookupFieldKind.Enum"/>.</summary>
    public Type EnumType { get; }
}
