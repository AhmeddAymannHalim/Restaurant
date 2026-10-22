using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests;

public class SupplierPaymentRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly SupplierPaymentRepository _sut;

    public SupplierPaymentRepositoryTests()
    {
        _sut = new SupplierPaymentRepository(_db.Context, new JournalEntryRepository(_db.Context));
    }

    public void Dispose() => _db.Dispose();

    // A supplier who owes 1000, with one open invoice of 500.
    private async Task<(Supplier supplier, BillMaster bill)> Seed(string code = "1")
    {
        var account = new Account { Code = code, ArName = "Cust" + code, EnName = "Cust" + code, AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit, CreditBalance = 1000 };
        var supplier = new Supplier { Account = account };
        var bill = new BillMaster
        {
            Code = "B" + code,
            Date = DateTime.Today,
            TotalBillAmountAfterDiscountAndExtra = 500,
            CasherRemainingAmount = 500,
            CustomerOrSupplierAccount = account
        };
        _db.Context.Suppliers.Add(supplier);
        _db.Context.BillMasters.Add(bill);
        await _db.Context.SaveChangesAsync();
        return (supplier, bill);
    }

    private async Task<string> SeedUser()
    {
        var user = new ApplicationUser { UserName = "boss", Email = "boss@test.com" };
        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();
        return user.Id;
    }

    private static PaymentInvoiceSupplier Payment(Supplier supplier, BillMaster bill, decimal paid) =>
        new() { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = paid, Description = "d" };

    private async Task<decimal> Balance(Supplier supplier) =>
        (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == supplier.AccountId)).BalanceValue;

    private async Task<BillMaster> Bill(BillMaster bill) =>
        await _db.Context.BillMasters.AsNoTracking().SingleAsync(a => a.Id == bill.Id);

    [Fact]
    public async Task Upsert_New_ReducesBalanceAndInvoiceRemaining()
    {
        var (supplier, bill) = await Seed();

        var result = await _sut.UpsertPaymentAsync(Payment(supplier, bill, 200));

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        Assert.Equal(800, await Balance(supplier));
        var saved = await Bill(bill);
        Assert.Equal(200, saved.AmountPaided);
        Assert.Equal(300, saved.CasherRemainingAmount);
    }

    [Fact]
    public async Task Upsert_Overpay_IsRejectedAndNothingChanges()
    {
        var (supplier, bill) = await Seed();

        var result = await _sut.UpsertPaymentAsync(Payment(supplier, bill, 501));

        Assert.False(result.Success);
        Assert.Equal("PaymentExceedsInvoiceRemainingAmount", result.Message);
        Assert.Equal(1000, await Balance(supplier));
        Assert.Empty(_db.Context.PaymentInvoiceSupplier);
    }

    [Fact]
    public async Task Upsert_PayExactlyRemaining_ClosesInvoice()
    {
        var (supplier, bill) = await Seed();

        Assert.True((await _sut.UpsertPaymentAsync(Payment(supplier, bill, 500))).Success);

        Assert.Equal(0, (await Bill(bill)).CasherRemainingAmount);
    }

    [Fact]
    public async Task Upsert_InvoiceOfAnotherSupplier_ReturnsRecordNotFound()
    {
        var (supplier, _) = await Seed("1");
        var (_, otherBill) = await Seed("2");

        var result = await _sut.UpsertPaymentAsync(Payment(supplier, otherBill, 10));

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task Upsert_UnknownSupplier_ReturnsRecordNotFound()
    {
        var (_, bill) = await Seed();

        var result = await _sut.UpsertPaymentAsync(new PaymentInvoiceSupplier { SupplierId = 99, BillMasterId = bill.Id, Paid = 10 });

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task Upsert_Edit_LowerAmount_GivesDifferenceBack()
    {
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 300);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var edit = Payment(supplier, bill, 100);
        edit.Id = payment.Id;
        var result = await _sut.UpsertPaymentAsync(edit);

        Assert.True(result.Success, result.Message);
        Assert.Equal(900, await Balance(supplier));
        Assert.Equal(400, (await Bill(bill)).CasherRemainingAmount);
    }

    [Fact]
    public async Task Upsert_Edit_UpToFullRemaining_IsAllowedButNotMore()
    {
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 300);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var tooMuch = Payment(supplier, bill, 501);
        tooMuch.Id = payment.Id;
        Assert.Equal("PaymentExceedsInvoiceRemainingAmount", (await _sut.UpsertPaymentAsync(tooMuch)).Message);
        _db.Context.ChangeTracker.Clear();

        var exact = Payment(supplier, bill, 500);
        exact.Id = payment.Id;
        Assert.True((await _sut.UpsertPaymentAsync(exact)).Success);
        Assert.Equal(500, await Balance(supplier));
    }

    [Fact]
    public async Task Upsert_Edit_UnknownPayment_ReturnsRecordNotFound()
    {
        var (supplier, bill) = await Seed();
        var edit = Payment(supplier, bill, 10);
        edit.Id = 55;

        Assert.Equal("RecordNotFound", (await _sut.UpsertPaymentAsync(edit)).Message);
    }

    [Fact]
    public async Task Upsert_PostsOneBalancedEntryAndEditReplacesIt()
    {
        var (supplier, bill) = await Seed();
        var box = new Account { Code = "BOX", ArName = "Box", EnName = "Box", AccountType = AccountType.Box, BalanceType = AccountBalanceType.Debit };
        _db.Context.EntrySettings.Add(new EntrySetting { EntryType = EntryType.SupplierPayment, ArName = "a", EnName = "b", Account = box });
        await _db.Context.SaveChangesAsync();
        var payment = Payment(supplier, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var edit = Payment(supplier, bill, 50);
        edit.Id = payment.Id;
        await _sut.UpsertPaymentAsync(edit);

        var entry = await _db.Context.EntryMasters.Include(a => a.EntryDetails).SingleAsync();
        Assert.Equal(50, entry.EntryDetails.Sum(a => a.Debit));
        Assert.Equal(50, entry.EntryDetails.Sum(a => a.Credit));
        Assert.Equal(-50, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == box.Id)).BalanceValue); // cash paid out
    }

    [Fact]
    public async Task Delete_RestoresBalanceInvoiceAndEntries()
    {
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 200);
        await _sut.UpsertPaymentAsync(payment);

        var result = await _sut.DeletePaymentAsync(payment.Id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(1000, await Balance(supplier));
        Assert.Equal(500, (await Bill(bill)).CasherRemainingAmount);
        Assert.Empty(_db.Context.PaymentInvoiceSupplier);
    }

    [Fact]
    public async Task Upsert_Edit_MovingThePaymentToAnotherInvoice_FreesTheFirstInvoice()
    {
        var (supplier, first) = await Seed();
        var second = new BillMaster { Code = "B2", Date = DateTime.Today, TotalBillAmountAfterDiscountAndExtra = 400, CasherRemainingAmount = 400, CustomerOrSupplierAccount = supplier.Account };
        _db.Context.BillMasters.Add(second);
        await _db.Context.SaveChangesAsync();
        var payment = Payment(supplier, first, 200);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var edit = Payment(supplier, second, 200);
        edit.Id = payment.Id;
        var result = await _sut.UpsertPaymentAsync(edit);

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, (await Bill(first)).AmountPaided);
        Assert.Equal(500, (await Bill(first)).CasherRemainingAmount);
        Assert.Equal(200, (await Bill(second)).AmountPaided);
        Assert.Equal(200, (await Bill(second)).CasherRemainingAmount);
        Assert.Equal(800, await Balance(supplier));
    }

    [Fact]
    public async Task GetRefundsForPayment_ListsOnlyThatPaymentsRefunds()
    {
        var userId = await SeedUser();
        var (supplier, bill) = await Seed();
        var first = Payment(supplier, bill, 200);
        var other = Payment(supplier, bill, 100);
        await _sut.UpsertPaymentAsync(first);
        await _sut.UpsertPaymentAsync(other);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = first.Id, Amount = 50 }, userId);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = other.Id, Amount = 10 }, userId);

        var refunds = (await _sut.GetRefundsForPaymentAsync(first.Id)).ToList();

        Assert.Equal(50, Assert.Single(refunds).Amount);
        Assert.Empty(await _sut.GetRefundsForPaymentAsync(999));
    }

    [Fact]
    public async Task Delete_Unknown_IsSuccess()
    {
        Assert.True((await _sut.DeletePaymentAsync(404)).Success);
    }

    [Fact]
    public async Task Delete_WithRefund_IsBlocked()
    {
        var userId = await SeedUser();
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 50 }, userId);

        var result = await _sut.DeletePaymentAsync(payment.Id);

        Assert.Equal("CannotDeletePaymentWithRefunds", result.Message);
        Assert.Single(_db.Context.PaymentInvoiceSupplier);
    }

    [Fact]
    public async Task RequestRefund_Rules()
    {
        var userId = await SeedUser();
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 200);
        await _sut.UpsertPaymentAsync(payment);

        Assert.Equal("RecordNotFound", (await _sut.RequestRefundAsync(new PaymentRefund { SupplierPaymentId = 99, Amount = 1 }, null)).Message);
        Assert.Equal("RefundAmountMustBePositive", (await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 0 }, null)).Message);
        Assert.Equal("RefundExceedsRemainingAmount", (await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 201 }, null)).Message);

        var ok = await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 150 }, userId);
        Assert.True(ok.Success, ok.Message);
        Assert.Equal("RefundExceedsRemainingAmount", (await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 51 }, null)).Message);
    }

    [Fact]
    public async Task RejectedRefund_FreesItsAmountAgain()
    {
        var userId = await SeedUser();
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 200 }, userId);
        var refundId = (await _db.Context.PaymentRefunds.SingleAsync()).Id;

        Assert.True((await _sut.RejectRefundAsync(refundId, userId)).Success);

        Assert.True((await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 200 }, null)).Success);
    }

    [Fact]
    public async Task ApproveRefund_GivesDebtBackAndCannotRunTwice()
    {
        var userId = await SeedUser();
        var (supplier, bill) = await Seed();
        var payment = Payment(supplier, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", SupplierPaymentId = payment.Id, Amount = 80 }, userId);
        var refundId = (await _db.Context.PaymentRefunds.SingleAsync()).Id;

        var first = await _sut.ApproveRefundAsync(refundId, userId);
        var second = await _sut.ApproveRefundAsync(refundId, userId);
        var rejectAfter = await _sut.RejectRefundAsync(refundId, userId);

        Assert.True(first.Success, first.Message);
        Assert.Equal(880, await Balance(supplier));
        Assert.Equal("RefundAlreadyProcessed", second.Message);
        Assert.Equal("RefundAlreadyProcessed", rejectAfter.Message);
        Assert.Equal(RefundStatus.Approved, (await _db.Context.PaymentRefunds.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task GetBalanceAndOpenInvoices_ReflectPayments()
    {
        var (supplier, bill) = await Seed();
        Assert.Equal(1000, await _sut.GetBalanceAsync(supplier.Id));
        Assert.Equal(0, await _sut.GetBalanceAsync(999));
        Assert.Single(await _sut.GetOpenInvoicesForPartnerAsync(supplier.Id));

        await _sut.UpsertPaymentAsync(Payment(supplier, bill, 500));

        Assert.Empty(await _sut.GetOpenInvoicesForPartnerAsync(supplier.Id));
        Assert.Empty(await _sut.GetOpenInvoicesForPartnerAsync(999));
    }

    [Fact]
    public async Task GetNextCode_IncrementsAfterEachPayment()
    {
        var (supplier, bill) = await Seed();
        Assert.Equal("1", await _sut.GetNextCodeAsync());

        await _sut.UpsertPaymentAsync(Payment(supplier, bill, 10));

        Assert.Equal("2", await _sut.GetNextCodeAsync());
    }
}
