using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests;

public class CustomerPaymentRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly CustomerPaymentRepository _sut;

    public CustomerPaymentRepositoryTests()
    {
        _sut = new CustomerPaymentRepository(_db.Context, new JournalEntryRepository(_db.Context));
    }

    public void Dispose() => _db.Dispose();

    // A customer who owes 1000, with one open invoice of 500.
    private async Task<(Customer customer, BillMaster bill)> Seed(string code = "1")
    {
        var account = new Account { Code = code, ArName = "Cust" + code, EnName = "Cust" + code, AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit, DebitBalance = 1000 };
        var customer = new Customer { Account = account };
        var setting = await new BillSupportRepository(_db.Context).GetBillSettingAsync(BillType.Sales, "مبيعات", "Sales");
        var bill = new BillMaster
        {
            BillSettingId = setting.Id,
            Code = "B" + code,
            Date = DateTime.Today,
            TotalBillAmountAfterDiscountAndExtra = 500,
            CasherRemainingAmount = 500,
            CustomerOrSupplierAccount = account
        };
        _db.Context.Customers.Add(customer);
        _db.Context.BillMasters.Add(bill);
        await _db.Context.SaveChangesAsync();
        return (customer, bill);
    }

    private async Task<string> SeedUser()
    {
        var user = new ApplicationUser { UserName = "boss", Email = "boss@test.com" };
        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();
        return user.Id;
    }

    private static PaymentInvoiceCustomer Payment(Customer customer, BillMaster bill, decimal paid) =>
        new() { CustomerId = customer.Id, BillMasterId = bill.Id, Paid = paid, Description = "d" };

    private async Task<decimal> Balance(Customer customer) =>
        (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == customer.AccountId)).BalanceValue;

    private async Task<BillMaster> Bill(BillMaster bill) =>
        await _db.Context.BillMasters.AsNoTracking().SingleAsync(a => a.Id == bill.Id);

    [Fact]
    public async Task Upsert_New_ReducesBalanceAndInvoiceRemaining()
    {
        var (customer, bill) = await Seed();

        var result = await _sut.UpsertPaymentAsync(Payment(customer, bill, 200));

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        Assert.Equal(800, await Balance(customer));
        var saved = await Bill(bill);
        Assert.Equal(200, saved.AmountPaided);
        Assert.Equal(300, saved.CasherRemainingAmount);
    }

    [Fact]
    public async Task Upsert_Overpay_IsRejectedAndNothingChanges()
    {
        var (customer, bill) = await Seed();

        var result = await _sut.UpsertPaymentAsync(Payment(customer, bill, 501));

        Assert.False(result.Success);
        Assert.Equal("PaymentExceedsInvoiceRemainingAmount", result.Message);
        Assert.Equal(1000, await Balance(customer));
        Assert.Empty(_db.Context.PaymentInvoiceCustomer);
    }

    [Fact]
    public async Task Upsert_PayExactlyRemaining_ClosesInvoice()
    {
        var (customer, bill) = await Seed();

        Assert.True((await _sut.UpsertPaymentAsync(Payment(customer, bill, 500))).Success);

        Assert.Equal(0, (await Bill(bill)).CasherRemainingAmount);
    }

    [Fact]
    public async Task Upsert_InvoiceOfAnotherCustomer_ReturnsRecordNotFound()
    {
        var (customer, _) = await Seed("1");
        var (_, otherBill) = await Seed("2");

        var result = await _sut.UpsertPaymentAsync(Payment(customer, otherBill, 10));

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task Upsert_UnknownCustomer_ReturnsRecordNotFound()
    {
        var (_, bill) = await Seed();

        var result = await _sut.UpsertPaymentAsync(new PaymentInvoiceCustomer { CustomerId = 99, BillMasterId = bill.Id, Paid = 10 });

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task Upsert_Edit_LowerAmount_GivesDifferenceBack()
    {
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 300);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var edit = Payment(customer, bill, 100);
        edit.Id = payment.Id;
        var result = await _sut.UpsertPaymentAsync(edit);

        Assert.True(result.Success, result.Message);
        Assert.Equal(900, await Balance(customer));
        Assert.Equal(400, (await Bill(bill)).CasherRemainingAmount);
    }

    [Fact]
    public async Task Upsert_Edit_UpToFullRemaining_IsAllowedButNotMore()
    {
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 300);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var tooMuch = Payment(customer, bill, 501);
        tooMuch.Id = payment.Id;
        Assert.Equal("PaymentExceedsInvoiceRemainingAmount", (await _sut.UpsertPaymentAsync(tooMuch)).Message);
        _db.Context.ChangeTracker.Clear();

        var exact = Payment(customer, bill, 500);
        exact.Id = payment.Id;
        Assert.True((await _sut.UpsertPaymentAsync(exact)).Success);
        Assert.Equal(500, await Balance(customer));
    }

    [Fact]
    public async Task Upsert_Edit_UnknownPayment_ReturnsRecordNotFound()
    {
        var (customer, bill) = await Seed();
        var edit = Payment(customer, bill, 10);
        edit.Id = 55;

        Assert.Equal("RecordNotFound", (await _sut.UpsertPaymentAsync(edit)).Message);
    }

    [Fact]
    public async Task Upsert_PostsOneBalancedEntryAndEditReplacesIt()
    {
        var (customer, bill) = await Seed();
        var box = new Account { Code = "BOX", ArName = "Box", EnName = "Box", AccountType = AccountType.Box, BalanceType = AccountBalanceType.Debit };
        _db.Context.EntrySettings.Add(new EntrySetting { EntryType = EntryType.CustomerPayment, ArName = "a", EnName = "b", Account = box });
        await _db.Context.SaveChangesAsync();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var edit = Payment(customer, bill, 50);
        edit.Id = payment.Id;
        await _sut.UpsertPaymentAsync(edit);

        var entry = await _db.Context.EntryMasters.Include(a => a.EntryDetails).SingleAsync();
        Assert.Equal(50, entry.EntryDetails.Sum(a => a.Debit));
        Assert.Equal(50, entry.EntryDetails.Sum(a => a.Credit));
        Assert.Equal(50, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == box.Id)).BalanceValue);
    }

    [Fact]
    public async Task Delete_RestoresBalanceInvoiceAndEntries()
    {
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);

        var result = await _sut.DeletePaymentAsync(payment.Id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(1000, await Balance(customer));
        Assert.Equal(500, (await Bill(bill)).CasherRemainingAmount);
        Assert.Empty(_db.Context.PaymentInvoiceCustomer);
    }

    [Fact]
    public async Task Upsert_Edit_MovingThePaymentToAnotherInvoice_FreesTheFirstInvoice()
    {
        var (customer, first) = await Seed();
        var second = new BillMaster { BillSettingId = first.BillSettingId, Code = "B2", Date = DateTime.Today, TotalBillAmountAfterDiscountAndExtra = 400, CasherRemainingAmount = 400, CustomerOrSupplierAccount = customer.Account };
        _db.Context.BillMasters.Add(second);
        await _db.Context.SaveChangesAsync();
        var payment = Payment(customer, first, 200);
        await _sut.UpsertPaymentAsync(payment);
        _db.Context.ChangeTracker.Clear();

        var edit = Payment(customer, second, 200);
        edit.Id = payment.Id;
        var result = await _sut.UpsertPaymentAsync(edit);

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, (await Bill(first)).AmountPaided);
        Assert.Equal(500, (await Bill(first)).CasherRemainingAmount);
        Assert.Equal(200, (await Bill(second)).AmountPaided);
        Assert.Equal(200, (await Bill(second)).CasherRemainingAmount);
        Assert.Equal(800, await Balance(customer));
    }

    [Fact]
    public async Task GetRefundsForPayment_ListsOnlyThatPaymentsRefunds()
    {
        var userId = await SeedUser();
        var (customer, bill) = await Seed();
        var first = Payment(customer, bill, 200);
        var other = Payment(customer, bill, 100);
        await _sut.UpsertPaymentAsync(first);
        await _sut.UpsertPaymentAsync(other);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = first.Id, Amount = 50 }, userId);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = other.Id, Amount = 10 }, userId);

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
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 50 }, userId);

        var result = await _sut.DeletePaymentAsync(payment.Id);

        Assert.Equal("CannotDeletePaymentWithRefunds", result.Message);
        Assert.Single(_db.Context.PaymentInvoiceCustomer);
    }

    [Fact]
    public async Task Edit_CannotLowerPaidBelowRefunds()
    {
        var userId = await SeedUser();
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 150 }, userId);
        _db.Context.ChangeTracker.Clear();

        var tooLow = Payment(customer, bill, 100);
        tooLow.Id = payment.Id;
        Assert.Equal("PaymentLessThanRefunds", (await _sut.UpsertPaymentAsync(tooLow)).Message);

        _db.Context.ChangeTracker.Clear();
        var enough = Payment(customer, bill, 150);
        enough.Id = payment.Id;
        Assert.True((await _sut.UpsertPaymentAsync(enough)).Success);
    }

    [Fact]
    public async Task RequestRefund_Rules()
    {
        var userId = await SeedUser();
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);

        Assert.Equal("RecordNotFound", (await _sut.RequestRefundAsync(new PaymentRefund { OriginalPaymentId = 99, Amount = 1 }, null)).Message);
        Assert.Equal("RefundAmountMustBePositive", (await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 0 }, null)).Message);
        Assert.Equal("RefundExceedsRemainingAmount", (await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 201 }, null)).Message);

        var ok = await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 150 }, userId);
        Assert.True(ok.Success, ok.Message);
        Assert.Equal("RefundExceedsRemainingAmount", (await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 51 }, null)).Message);
    }

    [Fact]
    public async Task RejectedRefund_FreesItsAmountAgain()
    {
        var userId = await SeedUser();
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 200 }, userId);
        var refundId = (await _db.Context.PaymentRefunds.SingleAsync()).Id;

        Assert.True((await _sut.RejectRefundAsync(refundId, userId)).Success);

        Assert.True((await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 200 }, null)).Success);
    }

    [Fact]
    public async Task ApproveRefund_GivesDebtBackAndCannotRunTwice()
    {
        var userId = await SeedUser();
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 80 }, userId);
        var refundId = (await _db.Context.PaymentRefunds.SingleAsync()).Id;

        var first = await _sut.ApproveRefundAsync(refundId, userId);
        var second = await _sut.ApproveRefundAsync(refundId, userId);
        var rejectAfter = await _sut.RejectRefundAsync(refundId, userId);

        Assert.True(first.Success, first.Message);
        Assert.Equal(880, await Balance(customer));
        Assert.Equal("RefundAlreadyProcessed", second.Message);
        Assert.Equal("RefundAlreadyProcessed", rejectAfter.Message);
        Assert.Equal(RefundStatus.Approved, (await _db.Context.PaymentRefunds.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task ApprovedRefund_MakesTheInvoiceOweAgain()
    {
        var userId = await SeedUser();
        var (customer, bill) = await Seed();
        var payment = Payment(customer, bill, 200);
        await _sut.UpsertPaymentAsync(payment);
        await _sut.RequestRefundAsync(new PaymentRefund { Reason = "r", OriginalPaymentId = payment.Id, Amount = 80 }, userId);
        var refundId = (await _db.Context.PaymentRefunds.SingleAsync()).Id;

        Assert.True((await _sut.ApproveRefundAsync(refundId, userId)).Success);

        var saved = await Bill(bill);
        Assert.Equal(120, saved.AmountPaided);
        Assert.Equal(380, saved.CasherRemainingAmount);
    }

    [Fact]
    public async Task OpenInvoices_ShowOnlySalesInvoices_AndPayingAnotherTypeIsRefused()
    {
        var (customer, bill) = await Seed();
        var otherSetting = await new BillSupportRepository(_db.Context).GetBillSettingAsync(BillType.SalesReturn, "مرتجع مبيعات", "SalesReturn");
        var other = new BillMaster { BillSettingId = otherSetting.Id, Code = "R1", Date = DateTime.Today, TotalBillAmountAfterDiscountAndExtra = 300, CasherRemainingAmount = 300, CustomerOrSupplierAccountId = bill.CustomerOrSupplierAccountId };
        _db.Context.BillMasters.Add(other);
        await _db.Context.SaveChangesAsync();

        var open = await _sut.GetOpenInvoicesForPartnerAsync(customer.Id);

        Assert.Equal(bill.Id, Assert.Single(open).Id);
        Assert.Equal("RecordNotFound", (await _sut.UpsertPaymentAsync(Payment(customer, other, 50))).Message);
    }

    [Fact]
    public async Task GetBalanceAndOpenInvoices_ReflectPayments()
    {
        var (customer, bill) = await Seed();
        Assert.Equal(1000, await _sut.GetBalanceAsync(customer.Id));
        Assert.Equal(0, await _sut.GetBalanceAsync(999));
        Assert.Single(await _sut.GetOpenInvoicesForPartnerAsync(customer.Id));

        await _sut.UpsertPaymentAsync(Payment(customer, bill, 500));

        Assert.Empty(await _sut.GetOpenInvoicesForPartnerAsync(customer.Id));
        Assert.Empty(await _sut.GetOpenInvoicesForPartnerAsync(999));
    }

    [Fact]
    public async Task GetNextCode_IncrementsAfterEachPayment()
    {
        var (customer, bill) = await Seed();
        Assert.Equal("1", await _sut.GetNextCodeAsync());

        await _sut.UpsertPaymentAsync(Payment(customer, bill, 10));

        Assert.Equal("2", await _sut.GetNextCodeAsync());
    }
}
