using Microsoft.Extensions.Logging;

namespace Shared.Helpers;

public static class ErrorLog
{
    public static ILogger Logger { get; set; }

    public static bool ShowDetails { get; set; }

    public static string Message(Exception ex, string fallback = "ErrorWhileSaving")
    {
        Logger?.LogError(ex, "Unhandled error");

        return ShowDetails ? ex.Message : fallback;
    }
}
