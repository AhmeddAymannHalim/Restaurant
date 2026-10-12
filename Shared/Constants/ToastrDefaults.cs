using Shared.Enums.Others;

namespace Shared.Constants;

public static class ToastrDefaults
{
    public const string ToasterList = "ToastrList";
    public const string Success = "success";
    public const string Info = "info";
    public const string Warning = "warning";
    public const string Error = "error";
}

public struct ToastrData
{
    public string Message { get; set; }
    public ToastrType Type { get; set; }
}
