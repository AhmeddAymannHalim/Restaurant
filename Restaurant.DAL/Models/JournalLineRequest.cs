namespace Restaurant.DAL.Models;

/// <summary>One side of an automatic posting, addressed by chart-of-accounts code.</summary>
public record JournalLineRequest(string AccountCode, decimal Debit, decimal Credit, string Description = null);
