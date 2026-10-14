namespace Shared.Enums;

public enum LookupFieldKind
{
    Text,
    Bool,
    Decimal,
    Int,
    Enum,

    /// <summary>Points at another row of the same table (ParentId).</summary>
    Parent,

    /// <summary>Points at a row of another lookup table (for example the currency of a company).</summary>
    Reference
}
