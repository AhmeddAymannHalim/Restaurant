using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// The chart of accounts, branch by branch: the list filter of every kind, what kind an account is, the code of the next account
// under the default parent of its kind, asset details, moving an account to another parent, and every reason a delete is refused.
public class AccountDefinitionBranchTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly AccountDefinitionRepository _sut;

    public AccountDefinitionBranchTests() => _sut = new AccountDefinitionRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private async Task<Account> Create(string name, int? parentId = null, AccountKind kind = AccountKind.GeneralLedger, AccountDetails details = null)
    {
        var account = new Account { ArName = name, EnName = name, ParentAccountId = parentId };
        var result = await _sut.UpsertAccountAsync(account, details, kind, 0);
        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();

        return await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id);
    }

    private async Task<AccountSetting> Settings(Account assets = null, Account expenses = null, Account banks = null, Account boxes = null)
    {
        var setting = new AccountSetting { ParentFixedAssetsId = assets?.Id, ParentExpensesId = expenses?.Id, ParentBankId = banks?.Id, ParentBoxId = boxes?.Id };
        _db.Context.AccountSettings.Add(setting);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        return setting;
    }

    private async Task<Account> Owned(string code, AccountType type, int? parentId = null)
    {
        var account = new Account { Code = code, ArName = code, EnName = code, AccountType = type, BalanceType = AccountBalanceType.Debit, ParentAccountId = parentId };
        _db.Context.Accounts.Add(account);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        return account;
    }

    // ---------------------------------------------------------------- list by kind

    [Fact]
    public async Task List_EveryKindShowsOnlyItsOwnAccounts_AndEachAccountKnowsItsKind()
    {
        var assetsParent = await Create("Fixed assets");
        var expensesParent = await Create("Expenses");
        var banksParent = await Create("Banks");
        var boxesParent = await Create("Boxes");
        await Settings(assetsParent, expensesParent, banksParent, boxesParent);
        var asset = await Create("Car", null, AccountKind.Asset, new AccountDetails { BuyValue = 100 });
        var rent = await Create("Rent", null, AccountKind.Expense);
        var bank = await Create("CIB", null, AccountKind.Bank);
        var safe = await Create("Main safe", null, AccountKind.Safe);
        var general = await Create("Capital");
        var customer = await Owned("9001", AccountType.Customer);

        async Task<List<string>> Names(string type, bool assetsOnly = false)
        {
            var page = await _sut.GetPagedAccountsAsync(new PageRequest { Type = type, PageSize = 100 }, assetsOnly);

            return page.Data.Select(a => a.EnName).ToList();
        }

        Assert.Equal(new[] { "Car" }, await Names(null, assetsOnly: true));
        Assert.Equal(new[] { "Car" }, await Names("Asset"));
        Assert.Equal(new[] { "CIB" }, await Names("Bank"));
        Assert.Equal(new[] { "Main safe" }, await Names("Safe"));
        Assert.Equal(new[] { "9001" }, await Names("Owned"));
        Assert.Contains("Rent", await Names("Expense"));
        Assert.DoesNotContain("Capital", await Names("Expense"));
        Assert.Contains("Capital", await Names("GeneralLedger"));
        Assert.DoesNotContain("Rent", await Names("GeneralLedger"));
        Assert.DoesNotContain("CIB", await Names("GeneralLedger"));
        Assert.Equal(10, (await Names(null)).Count);

        var all = (await _sut.GetPagedAccountsAsync(new PageRequest { PageSize = 100 }, false)).Data;
        Assert.Equal(AccountKind.Asset, all.Single(a => a.Id == asset.Id).Kind);
        Assert.Equal(AccountKind.Expense, all.Single(a => a.Id == rent.Id).Kind);
        Assert.Equal(AccountKind.Bank, all.Single(a => a.Id == bank.Id).Kind);
        Assert.Equal(AccountKind.Safe, all.Single(a => a.Id == safe.Id).Kind);
        Assert.Equal(AccountKind.GeneralLedger, all.Single(a => a.Id == general.Id).Kind);
        Assert.Equal(AccountKind.Owned, all.Single(a => a.Id == customer.Id).Kind);
    }

    [Fact]
    public async Task List_SearchesByCodeOrEitherName()
    {
        await Create("Cash box");
        await Create("Bank");

        Assert.Single((await _sut.GetPagedAccountsAsync(new PageRequest { SearchString = " CASH " }, false)).Data);
        Assert.Single((await _sut.GetPagedAccountsAsync(new PageRequest { SearchString = "2" }, false)).Data);
    }

    [Fact]
    public async Task GetById_GivesTheKindOfEveryAccount()
    {
        var expensesParent = await Create("Expenses");
        await Settings(null, expensesParent);
        var asset = await Create("Car", null, AccountKind.Asset, new AccountDetails());
        var rent = await Create("Rent", null, AccountKind.Expense);
        var bank = await Create("CIB", null, AccountKind.Bank);
        var safe = await Create("Safe", null, AccountKind.Safe);
        var general = await Create("Capital");
        var supplier = await Owned("9002", AccountType.Supplier);

        Assert.Equal(AccountKind.Asset, (await _sut.GetAccountByIdAsync(asset.Id)).Kind);
        Assert.Equal(AccountKind.Expense, (await _sut.GetAccountByIdAsync(rent.Id)).Kind);
        Assert.Equal(AccountKind.Bank, (await _sut.GetAccountByIdAsync(bank.Id)).Kind);
        Assert.Equal(AccountKind.Safe, (await _sut.GetAccountByIdAsync(safe.Id)).Kind);
        Assert.Equal(AccountKind.GeneralLedger, (await _sut.GetAccountByIdAsync(general.Id)).Kind);
        Assert.Equal(AccountKind.Owned, (await _sut.GetAccountByIdAsync(supplier.Id)).Kind);
        Assert.Null(await _sut.GetAccountByIdAsync(404));
    }

    // ---------------------------------------------------------------- the next code

    [Theory]
    [InlineData(AccountKind.Asset)]
    [InlineData(AccountKind.Expense)]
    [InlineData(AccountKind.Bank)]
    [InlineData(AccountKind.Safe)]
    public async Task NewCode_WithoutAParent_UsesTheDefaultParentOfTheKind(AccountKind kind)
    {
        var assets = await Create("Assets");
        var expenses = await Create("Expenses");
        var banks = await Create("Banks");
        var boxes = await Create("Boxes");
        await Settings(assets, expenses, banks, boxes);
        var parent = kind switch { AccountKind.Asset => assets, AccountKind.Expense => expenses, AccountKind.Bank => banks, _ => boxes };

        var code = await _sut.NewAccountCodeAsync(null, kind);

        Assert.Equal(parent.Code + "0001", code);
    }

    [Fact]
    public async Task NewCode_NoDefaultParentConfigured_OrAGeneralKind_IsTheNextMainNumber()
    {
        await Create("One");

        Assert.Equal("2", await _sut.NewAccountCodeAsync(null, AccountKind.GeneralLedger));
        Assert.Equal("2", await _sut.NewAccountCodeAsync(null, AccountKind.Bank));
    }

    [Fact]
    public async Task NewCode_SkipsANumberAnotherAccountAlreadyUses()
    {
        var one = await Create("One");
        // a child that happens to carry the code the next main account would get
        await Owned("2", AccountType.Normal, one.Id);
        await Owned("10001", AccountType.Normal, one.Id);

        Assert.Equal("3", await _sut.NewAccountCodeAsync(null, AccountKind.GeneralLedger));
        Assert.Equal("10002", await _sut.NewAccountCodeAsync(one.Id, AccountKind.GeneralLedger));
    }

    // ---------------------------------------------------------------- create

    [Fact]
    public async Task Create_WithoutAParent_UnderTheDefaultParentOfItsKind()
    {
        var banks = await Create("Banks");
        var boxes = await Create("Boxes");
        var expenses = await Create("Expenses");
        await Settings(null, expenses, banks, boxes);

        var bank = await Create("CIB", null, AccountKind.Bank);
        var safe = await Create("Safe", null, AccountKind.Safe);
        var rent = await Create("Rent", null, AccountKind.Expense);

        Assert.Equal(banks.Id, bank.ParentAccountId);
        Assert.Equal(AccountType.Bank, bank.AccountType);
        Assert.Equal(boxes.Id, safe.ParentAccountId);
        Assert.Equal(AccountType.Box, safe.AccountType);
        Assert.Equal(expenses.Id, rent.ParentAccountId);
        Assert.Equal(AccountType.Normal, rent.AccountType);
        Assert.Equal(AccountBalanceType.Debit, bank.BalanceType);
    }

    [Fact]
    public async Task Create_AnAsset_KeepsItsDetails_AndPointsToTheTopOfItsChain()
    {
        var top = await Create("Fixed assets");
        var middle = await Create("Vehicles", top.Id);
        var details = new AccountDetails
        {
            Location = "Garage", BuyValue = 500, BuyDate = new DateTime(2026, 1, 1), StartUsingDate = new DateTime(2026, 2, 1),
            ProductAge = 5, ScrapValue = 50, DepreciationType = DepreciationType.FixedInstallment
        };

        var car = await Create("Car", middle.Id, AccountKind.Asset, details);

        var saved = (await _db.Context.Accounts.AsNoTracking().Include(x => x.AccountDetails).SingleAsync(a => a.Id == car.Id)).AccountDetails;
        Assert.Equal("Garage", saved.Location);
        Assert.Equal(500, saved.BuyValue);
        Assert.Equal(middle.Id, saved.ParentAccountId);
        Assert.Equal(top.Id, saved.MainParentAccountId);
    }

    [Fact]
    public async Task Create_AnAssetWithNoDetailsGiven_StillGetsDefaultOnes()
    {
        var car = await Create("Car", null, AccountKind.Asset, null);

        var saved = await _db.Context.Accounts.AsNoTracking().Include(a => a.AccountDetails).SingleAsync(a => a.Id == car.Id);
        Assert.NotNull(saved.AccountDetails);
        Assert.Equal(DepreciationType.FixedInstallment, saved.AccountDetails.DepreciationType);
    }

    [Fact]
    public async Task Create_ASubAccountOfAGeneralParent_InheritsItsBalanceSide()
    {
        var liabilities = new Account { ArName = "Liabilities", EnName = "Liabilities" };
        await _sut.UpsertAccountAsync(liabilities, null, AccountKind.GeneralLedger, 0);
        var parent = await _db.Context.Accounts.SingleAsync(a => a.Id == liabilities.Id);
        parent.BalanceType = AccountBalanceType.Credit;
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var general = await Create("Suppliers", parent.Id);
        var bank = await Create("CIB", parent.Id, AccountKind.Bank);

        Assert.Equal(AccountBalanceType.Credit, general.BalanceType);
        Assert.Equal(AccountBalanceType.Debit, bank.BalanceType);
    }

    // ---------------------------------------------------------------- edit and move

    [Fact]
    public async Task Edit_UnknownOrOwnedAccounts_AreRefused_AndAnAccountCannotBeItsOwnParent()
    {
        var customer = await Owned("9003", AccountType.Customer);
        var one = await Create("One");

        var missing = await _sut.UpsertAccountAsync(new Account { Id = 404, ArName = "x", EnName = "x" }, null, AccountKind.GeneralLedger, 0);
        var owned = await _sut.UpsertAccountAsync(new Account { Id = customer.Id, ArName = "x", EnName = "x" }, null, AccountKind.GeneralLedger, 0);
        var itself = await _sut.UpsertAccountAsync(new Account { Id = one.Id, ArName = "One", EnName = "One", ParentAccountId = one.Id }, null, AccountKind.GeneralLedger, 0);

        Assert.Equal("RecordNotFound", missing.Message);
        Assert.Equal("AccountOwnedByAnotherScreen", owned.Message);
        Assert.Equal("InvalidParent", itself.Message);
    }

    [Fact]
    public async Task Edit_MovingAnAccount_CountsTheOldAndNewParent_AndNeverUnderItsOwnChildren()
    {
        var a = await Create("A");
        var b = await Create("B");
        var child = await Create("Child", a.Id);
        var grandChild = await Create("Grand", child.Id);
        _db.Context.ChangeTracker.Clear();

        var cycle = await _sut.UpsertAccountAsync(new Account { Id = a.Id, ArName = "A", EnName = "A", ParentAccountId = grandChild.Id }, null, AccountKind.GeneralLedger, 0);
        Assert.Equal("InvalidParent", cycle.Message);
        _db.Context.ChangeTracker.Clear();

        var moved = await _sut.UpsertAccountAsync(new Account { Id = child.Id, ArName = "Child", EnName = "Child", ParentAccountId = b.Id }, null, AccountKind.GeneralLedger, 0);

        Assert.True(moved.Success, moved.Message);
        _db.Context.ChangeTracker.Clear();
        var parents = await _db.Context.Accounts.AsNoTracking().ToListAsync();
        Assert.Equal(0, parents.Single(x => x.Id == a.Id).NumberOfSubAccounts);
        Assert.Equal(1, parents.Single(x => x.Id == b.Id).NumberOfSubAccounts);
        var movedAccount = parents.Single(x => x.Id == child.Id);
        Assert.Equal(b.Id, movedAccount.ParentAccountId);
        Assert.Equal(2, movedAccount.Level);

        // and back to the top
        _db.Context.ChangeTracker.Clear();
        var toTop = await _sut.UpsertAccountAsync(new Account { Id = child.Id, ArName = "Child", EnName = "Child", ParentAccountId = null }, null, AccountKind.GeneralLedger, 0);
        Assert.True(toTop.Success, toTop.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Null((await _db.Context.Accounts.AsNoTracking().SingleAsync(x => x.Id == child.Id)).ParentAccountId);
    }

    [Fact]
    public async Task Edit_AnAsset_ReplacesItsDetails_AndFollowsAMoveOfItsParent()
    {
        var top = await Create("Fixed assets");
        var vehicles = await Create("Vehicles", top.Id);
        var machines = await Create("Machines");
        var car = await Create("Car", vehicles.Id, AccountKind.Asset, new AccountDetails { Location = "Garage", BuyValue = 500 });
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.UpsertAccountAsync(
            new Account { Id = car.Id, ArName = "Car", EnName = "Car", ParentAccountId = machines.Id },
            new AccountDetails { Location = "Yard", BuyValue = 700, DepreciationType = DepreciationType.FixedInstallment },
            AccountKind.Asset, 0);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Accounts.AsNoTracking().Include(a => a.AccountDetails).SingleAsync(a => a.Id == car.Id);
        Assert.Equal("Yard", saved.AccountDetails.Location);
        Assert.Equal(700, saved.AccountDetails.BuyValue);
        Assert.Equal(machines.Id, saved.AccountDetails.ParentAccountId);
        Assert.Equal(machines.Id, saved.AccountDetails.MainParentAccountId);
        Assert.NotNull(saved.AccountDetails.UpdatedAt);
    }

    // ---------------------------------------------------------------- delete

    [Fact]
    public async Task Delete_UnknownIsFine_OwnedIsRefused_AndAnAssetTakesItsDetailsAndTheParentCountWithIt()
    {
        var parent = await Create("Fixed assets");
        var car = await Create("Car", parent.Id, AccountKind.Asset, new AccountDetails { BuyValue = 1 });
        var customer = await Owned("9004", AccountType.Customer);

        Assert.True((await _sut.DeleteAccountAsync(404)).Success);
        Assert.Equal("AccountOwnedByAnotherScreen", (await _sut.DeleteAccountAsync(customer.Id)).Message);

        var deleted = await _sut.DeleteAccountAsync(car.Id);

        Assert.True(deleted.Success, deleted.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Empty(_db.Context.AccountDetails);
        Assert.Equal(0, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == parent.Id)).NumberOfSubAccounts);
    }

    [Fact]
    public async Task Delete_IsRefusedWhileAnythingStillPointsAtTheAccount()
    {
        var withChild = await Create("Parent");
        await Create("Child", withChild.Id);
        var inEntry = await Create("In an entry");
        var onBill = await Create("On a bill");
        var inSetting = await Create("In the settings");
        var inBillSetting = await Create("In a bill setting");
        var inEntrySetting = await Create("In an entry setting");
        var depreciation = await Create("Depreciation");
        var boxOfEntry = await Create("Box of an entry");

        var master = new EntryMaster { Code = "E1", EntryDate = DateTime.Today, AccountBoxOrBankId = boxOfEntry.Id };
        master.EntryDetails.Add(new EntryDetail { AccountId = inEntry.Id, Debit = 1 });
        _db.Context.EntryMasters.Add(master);
        _db.Context.BillMasters.Add(new BillMaster { Code = "B1", Date = DateTime.Today, CustomerOrSupplierAccountId = onBill.Id });
        _db.Context.AccountSettings.Add(new AccountSetting { ParentExpensesId = inSetting.Id });
        _db.Context.AccountDetails.Add(new AccountDetails { DepreciationAccountId = depreciation.Id });
        await _db.Context.SaveChangesAsync();
        var sellType = new SellType { ArName = "s", EnName = "s", Show = true };
        _db.Context.SellTypes.Add(sellType);
        await _db.Context.SaveChangesAsync();
        _db.Context.BillSettings.Add(new BillSetting { ArName = "s", EnName = "s", ArAbrvation = "s", EnAbrvation = "s", SellTypeId = sellType.Id, AccountItemId = inBillSetting.Id });
        _db.Context.EntrySettings.Add(new EntrySetting { ArName = "e", EnName = "e", ArAbbreviation = "e", EnAbbreviation = "e", AccountId = inEntrySetting.Id });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        foreach (var linked in new[] { withChild, inEntry, onBill, inSetting, inBillSetting, inEntrySetting, depreciation, boxOfEntry })
        {
            var result = await _sut.DeleteAccountAsync(linked.Id);

            Assert.Equal("CannotDeleteLinkedRecord", result.Message);
        }
    }
}
