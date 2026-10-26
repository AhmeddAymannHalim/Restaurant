using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Allowance / fixed deduction / phone rows of an employee: which name and amount survive a create and an edit.
public class EmployeeChildRowTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly EmployeeRepository _sut;
    private Job _job;
    private AllowanceType _transport;
    private AllowanceType _meal;
    private DeductionType _penalty;
    private DeductionType _late;

    public EmployeeChildRowTests() => _sut = new EmployeeRepository(_db.Context, _db.Http);

    public void Dispose() => _db.Dispose();

    private async Task Seed()
    {
        _job = new Job { Code = "1", ArName = "Cook", EnName = "Cook" };
        _transport = new AllowanceType { Code = "1", ArName = "Transport", EnName = "Transport", Amount = 300 };
        _meal = new AllowanceType { Code = "2", ArName = "Meal", EnName = "Meal", Amount = 120 };
        _penalty = new DeductionType { Code = "1", ArName = "Penalty", EnName = "Penalty", Amount = 50 };
        _late = new DeductionType { Code = "2", ArName = "Late", EnName = "Late", Amount = 20 };
        _db.Context.AddRange(_job, _transport, _meal, _penalty, _late);
        await _db.Context.SaveChangesAsync();
    }

    private Employee NewEmployee() => new()
    {
        ArName = "Omar", EnName = "Omar", JobId = _job.Id, PhoneNumber = "01000000001",
        BasicSalary = 5200, WorkDays = 6, DailyHours = 8
    };

    private async Task<Employee> Created(Action<Employee> shape)
    {
        var employee = NewEmployee();
        shape(employee);
        var result = await _sut.UpsertEmployeeAsync(employee, null, null);
        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        return employee;
    }

    private async Task<ResultMessage> Edited(Employee created, Action<Employee> shape)
    {
        var edit = NewEmployee();
        edit.Id = created.Id;
        shape(edit);
        _db.Context.ChangeTracker.Clear();
        var result = await _sut.UpsertEmployeeAsync(edit, null, null);
        _db.Context.ChangeTracker.Clear();
        return result;
    }

    [Fact]
    public async Task Create_RowsWithAnUnknownType_AreNotSaved_NoAmountTypedByHandSurvives()
    {
        await Seed();

        await Created(e =>
        {
            e.Allowances = new List<EmployeeAllowance> { new() { AllowanceTypeId = 999, Amount = 77, ArName = "typed" } };
            e.FixedDeductions = new List<EmployeeFixedDeduction> { new() { DeductionTypeId = 999, Amount = 88, ArName = "typed" } };
        });

        Assert.Empty(_db.Context.EmployeeAllowances);
        Assert.Empty(_db.Context.EmployeeFixedDeductions);
    }

    [Fact]
    public async Task Edit_DeductionWithTheSameType_KeepsItsOldAmountEvenIfTheTypeChanged()
    {
        await Seed();
        var created = await Created(e => e.FixedDeductions = new List<EmployeeFixedDeduction> { new() { DeductionTypeId = _penalty.Id } });
        var type = await _db.Context.DeductionTypes.SingleAsync(a => a.Id == _penalty.Id);
        type.Amount = 999;
        await _db.Context.SaveChangesAsync();
        var rowId = (await _db.Context.EmployeeFixedDeductions.AsNoTracking().SingleAsync()).Id;

        var result = await Edited(created, e => e.FixedDeductions = new List<EmployeeFixedDeduction> { new() { Id = rowId, DeductionTypeId = _penalty.Id, Amount = 1 } });

        Assert.True(result.Success, result.Message);
        Assert.Equal(50, (await _db.Context.EmployeeFixedDeductions.AsNoTracking().SingleAsync()).Amount);
    }

    [Fact]
    public async Task Edit_RowsSwitchedToAnotherType_TakeTheNewTypesNameAndAmount()
    {
        await Seed();
        var created = await Created(e =>
        {
            e.Allowances = new List<EmployeeAllowance> { new() { AllowanceTypeId = _transport.Id } };
            e.FixedDeductions = new List<EmployeeFixedDeduction> { new() { DeductionTypeId = _penalty.Id } };
        });
        var allowanceId = (await _db.Context.EmployeeAllowances.AsNoTracking().SingleAsync()).Id;
        var deductionId = (await _db.Context.EmployeeFixedDeductions.AsNoTracking().SingleAsync()).Id;

        var result = await Edited(created, e =>
        {
            e.Allowances = new List<EmployeeAllowance> { new() { Id = allowanceId, AllowanceTypeId = _meal.Id, Amount = 1 } };
            e.FixedDeductions = new List<EmployeeFixedDeduction> { new() { Id = deductionId, DeductionTypeId = _late.Id, Amount = 1 } };
        });

        Assert.True(result.Success, result.Message);
        var allowance = await _db.Context.EmployeeAllowances.AsNoTracking().SingleAsync();
        var deduction = await _db.Context.EmployeeFixedDeductions.AsNoTracking().SingleAsync();
        Assert.Equal(120, allowance.Amount);
        Assert.Equal("Meal", allowance.EnName);
        Assert.Equal(20, deduction.Amount);
        Assert.Equal("Late", deduction.EnName);
    }

    [Fact]
    public async Task Edit_NewRowsFromTypes_AreAddedWithTheTypeAmount_AndUnknownTypesAreNotSaved()
    {
        await Seed();
        var created = await Created(e => { });

        var result = await Edited(created, e =>
        {
            e.Allowances = new List<EmployeeAllowance> { new() { AllowanceTypeId = _transport.Id }, new() { AllowanceTypeId = 999, Amount = 5 } };
            e.FixedDeductions = new List<EmployeeFixedDeduction> { new() { DeductionTypeId = _penalty.Id }, new() { DeductionTypeId = 999, Amount = 5 } };
        });

        Assert.True(result.Success, result.Message);
        var allowances = await _db.Context.EmployeeAllowances.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var deductions = await _db.Context.EmployeeFixedDeductions.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        Assert.Equal(new decimal[] { 300 }, allowances.Select(a => a.Amount).ToArray());
        Assert.Equal(new decimal[] { 50 }, deductions.Select(a => a.Amount).ToArray());
    }

    [Fact]
    public async Task Edit_Phones_AddsNewNumbersAndRemovesTheOnesLeftOut()
    {
        await Seed();
        var created = await Created(e => e.Phones = new List<EmployeePhone> { new() { PhoneNumber = "01222222222" }, new() { PhoneNumber = "01333333333" } });

        var result = await Edited(created, e => e.Phones = new List<EmployeePhone> { new() { PhoneNumber = "01222222222" }, new() { PhoneNumber = "01444444444" } });

        Assert.True(result.Success, result.Message);
        var phones = await _db.Context.EmployeePhones.AsNoTracking().Select(a => a.PhoneNumber).OrderBy(a => a).ToListAsync();
        Assert.Equal(new[] { "01000000001", "01222222222", "01444444444" }, phones);
    }

    [Fact]
    public async Task Edit_EmployeeWithoutAnAccount_GetsOne()
    {
        await Seed();
        var created = await Created(e => { });
        var employee = await _db.Context.Employees.SingleAsync();
        var accountId = employee.AccountId;
        employee.AccountId = null;
        await _db.Context.SaveChangesAsync();
        var oldAccount = await _db.Context.Accounts.SingleAsync(a => a.Id == accountId);
        _db.Context.Accounts.Remove(oldAccount);
        await _db.Context.SaveChangesAsync();

        var result = await Edited(created, e => e.ArName = "Omar Edited");

        Assert.True(result.Success, result.Message);
        var saved = await _db.Context.Employees.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.NotNull(saved.Account);
        Assert.Equal("Omar Edited", saved.Account.ArName);
    }
}
