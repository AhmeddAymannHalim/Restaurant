namespace Restaurant.DAL.Models;

public record JournalLineRequest(string AccountCode, decimal Debit, decimal Credit, string Description = null);
