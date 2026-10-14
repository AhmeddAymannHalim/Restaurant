using Restaurant.DAL.Models;

namespace Restaurant.DAL.Helpers;

public static class PhoneRows
{
    public static List<string> Clean(string primary, IEnumerable<IPhoneRow> extras)
    {
        var main = primary?.Trim();
        return (extras ?? Enumerable.Empty<IPhoneRow>())
            .Select(a => a.PhoneNumber?.Trim())
            .Where(a => !string.IsNullOrEmpty(a) && a != main)
            .Distinct()
            .ToList();
    }

    public static void Sync<T>(ApplicationDbContext context, ICollection<T> current, List<string> wanted) where T : class, IPhoneRow, new()
    {
        foreach (var row in current.Where(a => !wanted.Contains(a.PhoneNumber)).ToList())
        {
            context.Remove(row);
        }

        var kept = current.Select(a => a.PhoneNumber).ToHashSet();
        foreach (var number in wanted.Where(a => !kept.Contains(a)))
        {
            current.Add(new T { PhoneNumber = number });
        }
    }
}
