namespace Restaurant.DAL.Models;

// AccountId null = the main account of the voucher (EntrySetting.Account)
public record JournalLineRequest(int? AccountId, decimal Debit, decimal Credit, string Description = null);
