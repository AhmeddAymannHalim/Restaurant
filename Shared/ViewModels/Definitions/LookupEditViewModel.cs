using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

/// <summary>One shape for every simple lookup table: code, Arabic/English name and the table's extra columns.</summary>
public class LookupEditViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    /// <summary>Values of the extra columns, keyed by property name (see <see cref="LookupField"/>).</summary>
    public Dictionary<string, string> Extras { get; set; } = new();

    /// <summary>Drop-down options of the Parent / Reference fields, keyed by property name. Not posted.</summary>
    public Dictionary<string, IEnumerable<LookupItem>> Options { get; set; } = new();
}
