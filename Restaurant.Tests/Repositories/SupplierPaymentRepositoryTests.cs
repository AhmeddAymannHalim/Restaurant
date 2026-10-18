using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class SupplierPaymentRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (Account account, Supplier supplier) SeedSupplier(ApplicationDbContext context, decimal openingCredit = 0)
    {
        var account = new Account
        {
            Code = Guid.NewGuid().ToString("N")[..8],
            ArName = "مورد",
            EnName = "Supplier",
            BalanceType = AccountBalanceType.Credit,
            AccountType = AccountType.Supplier,
            CreditBalance = openingCredit,
            AccDate = DateTime.Today,
            AccCheckDate = DateTime.Today
        };
        var supplier = new Supplier { Account = account };
        context.Accounts.Add(account);
        context.Suppliers.Add(supplier);
        return (account, supplier);
    }

    private static BillMaster SeedBill(ApplicationDbContext context, int accountId, decimal remaining)
    {
        var bill = new BillMaster
        {
            Code = Guid.NewGuid().ToString("N"),
            Date = DateTime.Now,
            CustomerOrSupplierAccountId = accountId,
            TotalBillAmountAfterDiscountAndExtra = remaining,
            CasherRemainingAmount = remaining
        };
        context.BillMasters.Add(bill);
        return bill;
    }

    [Fact]
    public async Task UpsertPaymentAsync_NewPaymentWithinRemainingAmount_SucceedsAndReducesSupplierBalance()
    {
        // Arrange
        using var context = CreateContext();
        var (account, supplier) = SeedSupplier(context, openingCredit: 500);
        await context.SaveChangesAsync();
        var bill = SeedBill(context, account.Id, remaining: 500);
        await context.SaveChangesAsync();

        var journals = new JournalEntryRepository(context);
        var repository = new SupplierPaymentRepository(context, journals);
        var payment = new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = 150, Description = "test" };

        // Act
        var result = await repository.UpsertPaymentAsync(payment);

        // Assert
        Assert.True(result.Success);
        var updatedAccount = await context.Accounts.AsNoTracking().FirstAsync(a => a.Id == account.Id);
        Assert.Equal(350, updatedAccount.BalanceValue);
    }

    [Fact]
    public async Task UpsertPaymentAsync_PaymentExceedsBillRemainingAmount_ReturnsFailureWithoutChangingBalance()
    {
        // Arrange
        using var context = CreateContext();
        var (account, supplier) = SeedSupplier(context, openingCredit: 500);
        await context.SaveChangesAsync();
        var bill = SeedBill(context, account.Id, remaining: 80);
        await context.SaveChangesAsync();

        var journals = new JournalEntryRepository(context);
        var repository = new SupplierPaymentRepository(context, journals);
        var payment = new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = 150, Description = "too much" };

        // Act
        var result = await repository.UpsertPaymentAsync(payment);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("PaymentExceedsInvoiceRemainingAmount", result.Message);
        var unchangedAccount = await context.Accounts.AsNoTracking().FirstAsync(a => a.Id == account.Id);
        Assert.Equal(500, unchangedAccount.BalanceValue);
    }

    [Fact]
    public async Task DeletePaymentAsync_ExistingPaymentWithoutRefunds_RemovesPaymentAndRestoresBalance()
    {
        // Arrange
        using var context = CreateContext();
        var (account, supplier) = SeedSupplier(context, openingCredit: 500);
        await context.SaveChangesAsync();
        var bill = SeedBill(context, account.Id, remaining: 500);
        await context.SaveChangesAsync();

        var journals = new JournalEntryRepository(context);
        var repository = new SupplierPaymentRepository(context, journals);
        var upsertResult = await repository.UpsertPaymentAsync(new PaymentInvoiceSupplier
        {
            SupplierId = supplier.Id,
            BillMasterId = bill.Id,
            Paid = 150,
            Description = "to be deleted"
        });
        Assert.True(upsertResult.Success);
        var payment = await context.PaymentInvoiceSupplier.AsNoTracking().FirstAsync(a => a.SupplierId == supplier.Id);

        // Act
        var deleteResult = await repository.DeletePaymentAsync(payment.Id);

        // Assert
        Assert.True(deleteResult.Success);
        Assert.False(await context.PaymentInvoiceSupplier.AsNoTracking().AnyAsync(a => a.Id == payment.Id));
        var restoredAccount = await context.Accounts.AsNoTracking().FirstAsync(a => a.Id == account.Id);
        Assert.Equal(500, restoredAccount.BalanceValue);
    }

    [Fact]
    public async Task DeletePaymentAsync_PaymentDoesNotExist_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        var journals = new JournalEntryRepository(context);
        var repository = new SupplierPaymentRepository(context, journals);

        // Act
        var result = await repository.DeletePaymentAsync(999);

        // Assert
        Assert.True(result.Success);
    }
}
