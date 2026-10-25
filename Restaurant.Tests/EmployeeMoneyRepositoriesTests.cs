using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Deposit, ExitExpense, Salary and Scrounge: plain money records with a code, the user who made them and a date that never changes on edit.
public class EmployeeMoneyRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly JournalEntryRepository _journals;

    public EmployeeMoneyRepositoriesTests() => _journals = new JournalEntryRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private async Task<(string user, Employee employee, Expense expense)> Seed()
    {
        var user = new ApplicationUser { UserName = "boss", Email = "b@test.com" };
        var employee = new Employee { Code = "1", ArName = "Emp", EnName = "Emp", BasicSalary = 3000 };
        var expense = new Expense { Code = "1", ArName = "Rent", EnName = "Rent", Price = 100 };
        _db.Context.AddRange(user, employee, expense);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        return (user.Id, employee, expense);
    }

    // ---------------------------------------------------------------- deposit

    [Fact]
    public async Task Deposit_Create_AssignsCodeAndUser_EditKeepsThem()
    {
        var (user, _, _) = await Seed();
        var sut = new DepositRepository(_db.Context, _journals);
        var deposit = new Deposit { DeposerName = "Ali", Amount = 500, Description = "d" };

        Assert.True((await sut.UpsertDepositAsync(deposit, user)).Success);
        Assert.Equal("1", deposit.ProcessCode);
        _db.Context.ChangeTracker.Clear();

        var edit = new Deposit { Id = deposit.Id, DeposerName = "Omar", Amount = 700, ProcessCode = "HACK", UserId = "other" };
        Assert.True((await sut.UpsertDepositAsync(edit, "someone-else")).Success);

        _db.Context.ChangeTracker.Clear();
        var saved = await sut.GetDepositByIdAsync(deposit.Id);
        Assert.Equal("1", saved.ProcessCode);
        Assert.Equal(user, saved.UserId);
        Assert.Equal(700, saved.Amount);
        Assert.Equal("Omar", saved.DeposerName);
    }

    [Fact]
    public async Task Deposit_UnknownEdit_ReturnsRecordNotFound_AndDeleteIsIdempotent()
    {
        var (user, _, _) = await Seed();
        var sut = new DepositRepository(_db.Context, _journals);

        Assert.Equal("RecordNotFound", (await sut.UpsertDepositAsync(new Deposit { Id = 9, Amount = 1 }, user)).Message);
        Assert.True((await sut.DeleteDepositAsync(9)).Success);
    }

    [Fact]
    public async Task Deposit_Delete_RemovesTheRecord()
    {
        var (user, _, _) = await Seed();
        var sut = new DepositRepository(_db.Context, _journals);
        var deposit = new Deposit { DeposerName = "Ali", Amount = 500 };
        await sut.UpsertDepositAsync(deposit, user);

        Assert.True((await sut.DeleteDepositAsync(deposit.Id)).Success);
        Assert.Empty(_db.Context.Deposits);
        Assert.Equal("1", await sut.NewDepositCodeAsync());
    }

    // ---------------------------------------------------------------- exit expense

    [Fact]
    public async Task ExitExpense_Create_SetsDateAndUser_EditKeepsThem()
    {
        var (user, _, expense) = await Seed();
        var sut = new ExitExpenseRepository(_db.Context, _journals);
        var item = new ExitExpense { ExpenseId = expense.Id, Price = 80 };

        Assert.True((await sut.UpsertExitExpenseAsync(item, user)).Success);
        var date = (await sut.GetExitExpenseByIdAsync(item.Id)).Date;
        Assert.NotEqual(default, date);
        _db.Context.ChangeTracker.Clear();

        var edit = new ExitExpense { Id = item.Id, ExpenseId = expense.Id, Price = 95 };
        Assert.True((await sut.UpsertExitExpenseAsync(edit, "someone-else")).Success);

        _db.Context.ChangeTracker.Clear();
        var saved = await sut.GetExitExpenseByIdAsync(item.Id);
        Assert.Equal(95, saved.Price);
        Assert.Equal(date, saved.Date);
        Assert.Equal(user, saved.UserId);
    }

    [Fact]
    public async Task ExitExpense_UnknownEditAndDelete()
    {
        var (user, _, expense) = await Seed();
        var sut = new ExitExpenseRepository(_db.Context, _journals);

        Assert.Equal("RecordNotFound", (await sut.UpsertExitExpenseAsync(new ExitExpense { Id = 9, ExpenseId = expense.Id }, user)).Message);
        Assert.True((await sut.DeleteExitExpenseAsync(9)).Success);
    }

    [Fact]
    public async Task Expense_UsedByAnExitExpense_CannotBeDeleted()
    {
        var (user, _, expense) = await Seed();
        await new ExitExpenseRepository(_db.Context, _journals).UpsertExitExpenseAsync(new ExitExpense { ExpenseId = expense.Id, Price = 5 }, user);
        var expenses = new ExpenseRepository(_db.Context, _db.Http);

        var result = await expenses.DeleteExpenseAsync(expense.Id);

        Assert.False(result.Success);
        Assert.Equal("CannotDeleteLinkedRecord", result.Message);
    }

    // ---------------------------------------------------------------- scrounge and salary

    [Fact]
    public async Task Scrounge_Create_SetsUser_EditKeepsDate()
    {
        var (user, employee, _) = await Seed();
        var sut = new ScroungeRepository(_db.Context, _journals);
        var scrounge = new Scrounge { EmployeeId = employee.Id, Amount = 200, State = MoneyState.Dept, Info = "advance" };

        Assert.True((await sut.UpsertScroungeAsync(scrounge, user)).Success);
        var date = (await sut.GetScroungeByIdAsync(scrounge.Id)).Date;
        _db.Context.ChangeTracker.Clear();

        var edit = new Scrounge { Id = scrounge.Id, EmployeeId = employee.Id, Amount = 250, State = MoneyState.Discount, Date = new DateTime(2001, 1, 1) };
        Assert.True((await sut.UpsertScroungeAsync(edit, "someone-else")).Success);

        _db.Context.ChangeTracker.Clear();
        var saved = await sut.GetScroungeByIdAsync(scrounge.Id);
        Assert.Equal(250, saved.Amount);
        Assert.Equal(MoneyState.Discount, saved.State);
        Assert.Equal(date, saved.Date);
        Assert.Equal(user, saved.UserId);
    }

    [Fact]
    public async Task Salary_Create_AssignsCodeAndDate_AndNextCodeMoves()
    {
        var (user, employee, _) = await Seed();
        var sut = new SalaryRepository(_db.Context, _journals);
        var salary = new Salary { EmployeeId = employee.Id, FromDate = new DateTime(2030, 1, 1), ToDate = new DateTime(2030, 1, 31), SalaryAmount = 2500 };

        Assert.True((await sut.UpsertSalaryAsync(salary, user)).Success);

        var saved = await sut.GetSalaryByIdAsync(salary.Id);
        Assert.Equal("1", saved.SalaryCode);
        Assert.Equal(user, saved.UserId);
        Assert.Equal("2", await sut.NewSalaryCodeAsync());
    }

    [Fact]
    public async Task Salary_UnknownEdit_ReturnsRecordNotFound_DeleteWorks()
    {
        var (user, employee, _) = await Seed();
        var sut = new SalaryRepository(_db.Context, _journals);
        var salary = new Salary { EmployeeId = employee.Id, SalaryAmount = 100 };
        await sut.UpsertSalaryAsync(salary, user);

        Assert.Equal("RecordNotFound", (await sut.UpsertSalaryAsync(new Salary { Id = 9, EmployeeId = employee.Id }, user)).Message);
        Assert.True((await sut.DeleteSalaryAsync(salary.Id)).Success);
        Assert.Empty(_db.Context.Salaries);
    }

    [Fact]
    public async Task EmployeeExpenses_AddsIncentivesAndSubtractsDiscountsAndDebts_OnlyInsideThePeriod()
    {
        var (user, employee, _) = await Seed();
        var scrounges = new ScroungeRepository(_db.Context, _journals);
        _db.Context.Scrounges.AddRange(
            new Scrounge { EmployeeId = employee.Id, Amount = 100, State = MoneyState.Incentive, Date = new DateTime(2030, 1, 5), UserId = user },
            new Scrounge { EmployeeId = employee.Id, Amount = 40, State = MoneyState.Discount, Date = new DateTime(2030, 1, 10), UserId = user },
            new Scrounge { EmployeeId = employee.Id, Amount = 60, State = MoneyState.Dept, Date = new DateTime(2030, 1, 31, 23, 0, 0), UserId = user },
            new Scrounge { EmployeeId = employee.Id, Amount = 999, State = MoneyState.Dept, Date = new DateTime(2030, 2, 1), UserId = user });
        await _db.Context.SaveChangesAsync();
        var sut = new SalaryRepository(_db.Context, _journals);

        var model = await sut.GetEmployeeExpensesAsync(employee.Id, new DateTime(2030, 1, 1), new DateTime(2030, 1, 31));

        Assert.Equal(100, model.Incentives);
        Assert.Equal(40, model.Discounts);
        Assert.Equal(60, model.Depts);
        Assert.Equal(3000, model.MainSalary);
        Assert.Equal(3000 + 100 - 40 - 60, model.TotalSalary);
        Assert.Null(await sut.GetEmployeeExpensesAsync(404, DateTime.Today, DateTime.Today));
    }

    [Fact]
    public async Task EmployeeExpenses_AddsAllowancesAndSubtractsFixedDeductions()
    {
        var (user, employee, _) = await Seed();
        _db.Context.EmployeeAllowances.Add(new EmployeeAllowance { EmployeeId = employee.Id, Amount = 300, ArName = "بدل", EnName = "Transport" });
        _db.Context.EmployeeFixedDeductions.Add(new EmployeeFixedDeduction { EmployeeId = employee.Id, Amount = 120, ArName = "خصم", EnName = "Late" });
        _db.Context.Scrounges.AddRange(
            new Scrounge { EmployeeId = employee.Id, Amount = 100, State = MoneyState.Incentive, Date = new DateTime(2030, 1, 5), UserId = user },
            new Scrounge { EmployeeId = employee.Id, Amount = 50, State = MoneyState.Discount, Date = new DateTime(2030, 1, 6), UserId = user },
            new Scrounge { EmployeeId = employee.Id, Amount = 200, State = MoneyState.Dept, Date = new DateTime(2030, 1, 7), UserId = user });
        await _db.Context.SaveChangesAsync();
        var sut = new SalaryRepository(_db.Context, _journals);

        var model = await sut.GetEmployeeExpensesAsync(employee.Id, new DateTime(2030, 1, 1), new DateTime(2030, 1, 31));

        Assert.Equal(300, model.Allowances);
        Assert.Equal(120, model.FixedDeductions);
        Assert.Equal(3000 + 300 + 100 - 120 - 50 - 200, model.TotalSalary);
    }

    [Fact]
    public async Task Salary_StoppedEmployee_CannotGetNewSalary_ButSavedOneStaysEditable()
    {
        var (user, employee, _) = await Seed();
        var sut = new SalaryRepository(_db.Context, _journals);
        var salary = new Salary { EmployeeId = employee.Id, FromDate = new DateTime(2030, 1, 1), ToDate = new DateTime(2030, 1, 31), SalaryAmount = 2500 };
        Assert.True((await sut.UpsertSalaryAsync(salary, user)).Success);

        employee.IsSalaryStopped = true;
        _db.Context.Employees.Update(employee);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var blocked = await sut.UpsertSalaryAsync(new Salary { EmployeeId = employee.Id, SalaryAmount = 1 }, user);
        Assert.False(blocked.Success);
        Assert.Equal("SalaryStopped", blocked.Message);
        Assert.Single(_db.Context.Salaries.AsNoTracking());
        Assert.True((await sut.GetEmployeeExpensesAsync(employee.Id, DateTime.Today, DateTime.Today)).IsSalaryStopped);

        var edit = new Salary { Id = salary.Id, EmployeeId = employee.Id, FromDate = salary.FromDate, ToDate = salary.ToDate, SalaryAmount = 2600 };
        Assert.Equal("SalaryStopped", (await sut.UpsertSalaryAsync(edit, user)).Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(2500, (await sut.GetSalaryByIdAsync(salary.Id)).SalaryAmount);
    }
    [Fact]
    public async Task Paging_ReturnsRecordsForAllFour()
    {
        var (user, employee, expense) = await Seed();
        await new DepositRepository(_db.Context, _journals).UpsertDepositAsync(new Deposit { DeposerName = "Ali", Amount = 1 }, user);
        await new ExitExpenseRepository(_db.Context, _journals).UpsertExitExpenseAsync(new ExitExpense { ExpenseId = expense.Id, Price = 1 }, user);
        await new ScroungeRepository(_db.Context, _journals).UpsertScroungeAsync(new Scrounge { EmployeeId = employee.Id, Amount = 1 }, user);
        await new SalaryRepository(_db.Context, _journals).UpsertSalaryAsync(new Salary { EmployeeId = employee.Id, SalaryAmount = 1 }, user);
        var request = new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) };

        Assert.Single((await new DepositRepository(_db.Context, _journals).GetPagedDepositsAsync(request)).Data);
        Assert.Single((await new ExitExpenseRepository(_db.Context, _journals).GetPagedExitExpensesAsync(request)).Data);
        Assert.Single((await new ScroungeRepository(_db.Context, _journals).GetPagedScroungesAsync(request)).Data);
        Assert.Single((await new SalaryRepository(_db.Context, _journals).GetPagedSalariesAsync(request)).Data);
    }
}
