using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The chart of accounts, the account settings and the helper that creates customer / supplier / employee accounts.
public class AccountRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly AccountDefinitionRepository _sut;

    public AccountRepositoriesTests() => _sut = new AccountDefinitionRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private async Task<Account> Create(string name, int? parentId = null, AccountKind kind = AccountKind.GeneralLedger, decimal opening = 0, AccountDetails details = null)
    {
        var account = new Account { ArName = name, EnName = name, ParentAccountId = parentId };
        var result = await _sut.UpsertAccountAsync(account, details, kind, opening);
        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        return await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id);
    }

    [Fact]
    public async Task Create_RootAccounts_GetNextFreeNumber()
    {
        var first = await Create("Assets");
        var second = await Create("Liabilities");

        Assert.Equal("1", first.Code);
        Assert.Equal("2", second.Code);
        Assert.Equal(1, first.Level);
        Assert.Equal(AccountBalanceType.Debit, first.BalanceType);
    }

    [Fact]
    public async Task Create_Child_GetsParentCodePlusNumber_AndCountsOnTheParent()
    {
        var parent = await Create("Assets");

        var child1 = await Create("Cash", parent.Id);
        var child2 = await Create("Bank", parent.Id);

        Assert.Equal("10001", child1.Code);
        Assert.Equal("10002", child2.Code);
        Assert.Equal(2, child1.Level);
        Assert.Equal(2, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == parent.Id)).NumberOfSubAccounts);
    }

    [Fact]
    public async Task Create_ChildCodeDoesNotPushTheNextRootNumber()
    {
        var parent = await Create("Assets");
        await Create("Cash", parent.Id);

        var root = await Create("Liabilities");

        Assert.Equal("2", root.Code);
    }

    [Fact]
    public async Task Create_OpeningBalance_IsStoredOnTheAccountSide()
    {
        var parent = await Create("Assets");

        var account = await Create("Cash", parent.Id, opening: 300);

        Assert.Equal(300, account.BalanceValue);
        Assert.Equal(300, account.DebitBalance);
    }

    [Fact]
    public async Task Create_Asset_KeepsDetailsAndFindsTheTopParent()
    {
        var top = await Create("Assets");
        var group = await Create("Fixed", top.Id);

        var asset = await Create("Car", group.Id, AccountKind.Asset, details: new AccountDetails { Location = "Garage", BuyValue = 9000 });

        var details = await _db.Context.AccountDetails.AsNoTracking().SingleAsync();
        Assert.Equal("Garage", details.Location);
        Assert.Equal(group.Id, details.ParentAccountId);
        Assert.Equal(top.Id, details.MainParentAccountId);
        Assert.Equal(asset.Id, details.AccountId);
    }

    [Fact]
    public async Task Create_BankAndSafe_UseTheDefaultParentFromTheSettings()
    {
        var banks = await Create("Banks");
        var boxes = await Create("Boxes");
        _db.Context.AccountSettings.Add(new AccountSetting { ParentBankId = banks.Id, ParentBoxId = boxes.Id });
        await _db.Context.SaveChangesAsync();

        var bank = await Create("CIB", kind: AccountKind.Bank);
        var safe = await Create("Main safe", kind: AccountKind.Safe);

        Assert.Equal(banks.Id, bank.ParentAccountId);
        Assert.Equal(AccountType.Bank, bank.AccountType);
        Assert.Equal(boxes.Id, safe.ParentAccountId);
        Assert.Equal(AccountType.Box, safe.AccountType);
    }

    [Fact]
    public async Task NewCode_MatchesWhatCreateWillUse()
    {
        var parent = await Create("Assets");
        await Create("Cash", parent.Id);

        Assert.Equal("10002", await _sut.NewAccountCodeAsync(parent.Id, AccountKind.GeneralLedger));
        Assert.Equal("2", await _sut.NewAccountCodeAsync(null, AccountKind.GeneralLedger));
    }

    [Fact]
    public async Task Edit_ChangesNamesOnly_AndKeepsCodeAndBalance()
    {
        var parent = await Create("Assets");
        var account = await Create("Cash", parent.Id, opening: 100);
        var edit = new Account { Id = account.Id, ParentAccountId = parent.Id, ArName = "New", EnName = "New", Code = "HACK" };

        var result = await _sut.UpsertAccountAsync(edit, null, AccountKind.GeneralLedger, 9999);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id);
        Assert.Equal("New", saved.EnName);
        Assert.Equal(account.Code, saved.Code);
        Assert.Equal(100, saved.BalanceValue);
    }

    [Fact]
    public async Task Edit_MoveUnderAnotherParent_UpdatesCountersAndLevel()
    {
        var a = await Create("A");
        var b = await Create("B");
        var child = await Create("Child", a.Id);

        var result = await _sut.UpsertAccountAsync(new Account { Id = child.Id, ParentAccountId = b.Id, ArName = "Child", EnName = "Child" }, null, AccountKind.GeneralLedger, 0);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(0, (await _db.Context.Accounts.AsNoTracking().SingleAsync(x => x.Id == a.Id)).NumberOfSubAccounts);
        Assert.Equal(1, (await _db.Context.Accounts.AsNoTracking().SingleAsync(x => x.Id == b.Id)).NumberOfSubAccounts);
    }

    [Fact]
    public async Task Edit_OwnParentOrADescendantAsParent_IsRejected()
    {
        var root = await Create("Root");
        var child = await Create("Child", root.Id);
        var grandChild = await Create("Grand", child.Id);

        var self = await _sut.UpsertAccountAsync(new Account { Id = child.Id, ParentAccountId = child.Id, ArName = "x", EnName = "x" }, null, AccountKind.GeneralLedger, 0);
        var loop = await _sut.UpsertAccountAsync(new Account { Id = root.Id, ParentAccountId = grandChild.Id, ArName = "x", EnName = "x" }, null, AccountKind.GeneralLedger, 0);

        Assert.Equal("InvalidParent", self.Message);
        Assert.Equal("InvalidParent", loop.Message);
    }

    [Fact]
    public async Task Edit_UnknownAccount_ReturnsRecordNotFound()
    {
        var result = await _sut.UpsertAccountAsync(new Account { Id = 77, ArName = "x", EnName = "x" }, null, AccountKind.GeneralLedger, 0);

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task OwnedAccounts_CannotBeEditedOrDeletedHere()
    {
        var customer = new Account { Code = "C1", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer };
        _db.Context.Accounts.Add(customer);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var edit = await _sut.UpsertAccountAsync(new Account { Id = customer.Id, ArName = "x", EnName = "x" }, null, AccountKind.GeneralLedger, 0);
        var delete = await _sut.DeleteAccountAsync(customer.Id);

        Assert.Equal("AccountOwnedByAnotherScreen", edit.Message);
        Assert.Equal("AccountOwnedByAnotherScreen", delete.Message);
        Assert.DoesNotContain(customer.Id, (await _sut.GetParentOptionsAsync(null)).Select(a => a.Id));
        Assert.DoesNotContain(customer.Id, (await _sut.GetPostingAccountOptionsAsync()).Select(a => a.Id));
    }

    [Fact]
    public async Task Delete_Leaf_RemovesItAndLowersTheParentCounter()
    {
        var parent = await Create("Assets");
        var child = await Create("Cash", parent.Id);

        var result = await _sut.DeleteAccountAsync(child.Id);

        Assert.True(result.Success, result.Message);
        Assert.Single(_db.Context.Accounts);
        Assert.Equal(0, (await _db.Context.Accounts.AsNoTracking().SingleAsync()).NumberOfSubAccounts);
    }

    [Fact]
    public async Task Delete_WithChildrenOrEntriesOrSettings_IsBlocked()
    {
        var parent = await Create("Assets");
        var used = await Create("Cash", parent.Id);
        var inSettings = await Create("Sales", null);
        _db.Context.EntryMasters.Add(new EntryMaster
        {
            Code = "1", EntryDate = DateTime.Today,
            EntrySetting = new EntrySetting { EntryType = EntryType.Journal, ArName = "a", EnName = "b" },
            EntryDetails = new List<EntryDetail> { new() { AccountId = used.Id, Debit = 5 } }
        });
        _db.Context.BillSettings.Add(new BillSetting { ArName = "a", EnName = "b", BillType = BillType.Sales, SellTypeId = (await SellType()).Id, AccountItemId = inSettings.Id });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        Assert.Equal("CannotDeleteLinkedRecord", (await _sut.DeleteAccountAsync(parent.Id)).Message);
        Assert.Equal("CannotDeleteLinkedRecord", (await _sut.DeleteAccountAsync(used.Id)).Message);
        Assert.Equal("CannotDeleteLinkedRecord", (await _sut.DeleteAccountAsync(inSettings.Id)).Message);
        Assert.True((await _sut.DeleteAccountAsync(404)).Success);
    }

    private async Task<SellType> SellType()
    {
        var sellType = new SellType { ArName = "r", EnName = "r" };
        _db.Context.SellTypes.Add(sellType);
        await _db.Context.SaveChangesAsync();
        return sellType;
    }

    [Fact]
    public async Task Paging_SplitsAccountsByKind()
    {
        var root = await Create("General");
        var bank = await Create("CIB", kind: AccountKind.Bank);
        var safe = await Create("Safe", kind: AccountKind.Safe);
        _db.Context.Accounts.Add(new Account { Code = "C1", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer });
        await _db.Context.SaveChangesAsync();

        Assert.Equal(AccountKind.Bank, (await _sut.GetPagedAccountsAsync(new PageRequest { Type = "Bank" }, false)).Data.Single().Kind);
        Assert.Equal(AccountKind.Safe, (await _sut.GetPagedAccountsAsync(new PageRequest { Type = "Safe" }, false)).Data.Single().Kind);
        Assert.Equal(AccountKind.Owned, (await _sut.GetPagedAccountsAsync(new PageRequest { Type = "Owned" }, false)).Data.Single().Kind);
        Assert.Equal(AccountKind.GeneralLedger, (await _sut.GetPagedAccountsAsync(new PageRequest { Type = "GeneralLedger" }, false)).Data.Single().Kind);
        Assert.Equal(4, (await _sut.GetPagedAccountsAsync(new PageRequest(), false)).TotalRecords);
        Assert.Single((await _sut.GetPagedAccountsAsync(new PageRequest { SearchString = "cib" }, false)).Data);
        Assert.Equal(AccountKind.Bank, (await _sut.GetAccountByIdAsync(bank.Id)).Kind);
        Assert.Null(await _sut.GetAccountByIdAsync(404));
    }

    // ---------------------------------------------------------------- provision

    [Fact]
    public async Task Provision_Customer_IsDebitAndUnderTheSettingsParent()
    {
        var parent = await Create("Customers");
        _db.Context.AccountSettings.Add(new AccountSetting { ParentCustomerId = parent.Id });
        await _db.Context.SaveChangesAsync();
        var sut = new AccountProvisionRepository(_db.Context);

        var account = await sut.CreateAsync("عميل", "Customer", AccountType.Customer, 250);
        await _db.Context.SaveChangesAsync();

        Assert.Equal("10001", account.Code);
        Assert.Equal(parent.Id, account.ParentAccountId);
        Assert.Equal(AccountBalanceType.Debit, account.BalanceType);
        Assert.Equal(250, account.BalanceValue);
    }

    [Fact]
    public async Task Provision_Supplier_IsCredit_AndNumbersChildrenOneByOne()
    {
        var parent = await Create("Suppliers");
        _db.Context.AccountSettings.Add(new AccountSetting { ParentSupplierId = parent.Id });
        await _db.Context.SaveChangesAsync();
        var sut = new AccountProvisionRepository(_db.Context);

        var first = await sut.CreateAsync("م1", "S1", AccountType.Supplier);
        await _db.Context.SaveChangesAsync();
        var second = await sut.CreateAsync("م2", "S2", AccountType.Supplier);
        await _db.Context.SaveChangesAsync();

        Assert.Equal(AccountBalanceType.Credit, first.BalanceType);
        Assert.Equal("10001", first.Code);
        Assert.Equal("10002", second.Code);
    }

    [Fact]
    public async Task Provision_WithoutParent_TakesTheNextMainNumber_NotPushedByChildren()
    {
        var parent = await Create("Assets");
        await Create("Cash", parent.Id);
        var sut = new AccountProvisionRepository(_db.Context);

        var account = await sut.CreateAsync("عميل", "Customer", AccountType.Customer);

        Assert.Equal("2", account.Code);
    }

    // ---------------------------------------------------------------- account settings

    [Fact]
    public async Task AccountSetting_SaveAndLoad_RoundTrips()
    {
        var sut = new AccountSettingRepository(_db.Context);
        var sales = await Create("Sales");
        var cash = await Create("Cash");

        var model = await sut.GetAccountSettingAsync();
        model.SalesItemAccountId = sales.Id;
        model.SalesPayAccountId = cash.Id;
        model.CustomerPaymentAccountId = cash.Id;
        model.ParentCustomerId = sales.Id;
        var result = await sut.UpdateAccountSettingAsync(model);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var loaded = await sut.GetAccountSettingAsync();
        Assert.Equal(sales.Id, loaded.SalesItemAccountId);
        Assert.Equal(cash.Id, loaded.SalesPayAccountId);
        Assert.Equal(cash.Id, loaded.CustomerPaymentAccountId);
        Assert.Equal(sales.Id, loaded.ParentCustomerId);
        Assert.Equal(2, loaded.Accounts.Count());
        Assert.Single(_db.Context.AccountSettings);
    }
}
