using System.ComponentModel;

namespace Shared.Enums;

public enum SupportedInvoiceTypes
{
    [Description("0100")]
    Simplified,

    [Description("1000")]
    Standard,

    [Description("1100")]
    Both
}
