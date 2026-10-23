using System.Drawing.Printing;
using Shared.ViewModels.Commons;

namespace ResturantPL.Helper;

// The printers of the machine the app runs on, as a list the printer setting can pick from.
public static class InstalledPrinters
{
    public static List<LookupItem_String> List()
    {
        var printers = new List<LookupItem_String>();
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            printers.AddRange(PrinterSettings.InstalledPrinters.Cast<string>()
                .Select(a => new LookupItem_String { Id = a, ArName = a, EnName = a }));
        }

        return printers;
    }
}
