using Shared.Enums;

namespace Restaurant.DAL.Models;

public class JournalEntry
{
    public int Id { get; set; }
    public string EntryNumber { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Description { get; set; }
    public JournalSourceType SourceType { get; set; }
    public int? SourceId { get; set; }
    public string CreatedByUserId { get; set; }

    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
