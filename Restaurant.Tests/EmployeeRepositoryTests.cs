using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

public class EmployeeRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly EmployeeRepository _sut;

    public EmployeeRepositoryTests() => _sut = new EmployeeRepository(_db.Context, _db.Http);

    public void Dispose() => _db.Dispose();

    private async Task<Job> SeedJob(string name = "Cook")
    {
        var job = new Job { Code = "1", ArName = name, EnName = name };
        _db.Context.Jobs.Add(job);
        await _db.Context.SaveChangesAsync();
        return job;
    }

    private static Employee NewEmployee(Job job, string name = "Omar") => new()
    {
        ArName = name, EnName = name, JobId = job.Id, PhoneNumber = "01000000001",
        BasicSalary = 5200, WorkDays = 6, DailyHours = 8
    };

    [Fact]
    public async Task Create_AssignsCodeAndAnEmployeeAccount()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);

        var result = await _sut.UpsertEmployeeAsync(employee, null, null);

        Assert.True(result.Success, result.Message);
        var saved = await _db.Context.Employees.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.Equal("1", saved.Code);
        Assert.Equal(AccountType.Employee, saved.Account.AccountType);
        Assert.Equal(AccountBalanceType.Credit, saved.Account.BalanceType);
        Assert.Equal("Omar", saved.Account.EnName);
        Assert.NotNull(saved.CreatedAt);
    }

    [Fact]
    public async Task Create_CalculatesHoursRatesAndInsurance()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.InsuranceBasicSalary = 3000;
        employee.InsuranceAllowancesAmount = 1000;
        employee.EmployeeInsurancePercent = 11;
        employee.CompanyInsurancePercent = 18.75m;

        await _sut.UpsertEmployeeAsync(employee, null, null);

        var saved = await _db.Context.Employees.AsNoTracking().SingleAsync();
        Assert.Equal(48, saved.WeeklyHours);
        Assert.Equal(208, saved.MonthlyHours);          // 48 * 52 / 12
        Assert.Equal(200, saved.DailyRate);             // 5200 / (6 * 52 / 12)
        Assert.Equal(25, saved.HourlyRate);
        Assert.Equal(0.4167m, saved.MinuteRate);
        Assert.Equal(440, saved.EmployeeInsuranceAmount);
        Assert.Equal(750, saved.CompanyInsuranceAmount);
    }

    [Fact]
    public async Task Create_WithoutWorkDays_HasNoRates()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.WorkDays = null;
        employee.DailyHours = null;

        await _sut.UpsertEmployeeAsync(employee, null, null);

        var saved = await _db.Context.Employees.AsNoTracking().SingleAsync();
        Assert.Null(saved.DailyRate);
        Assert.Null(saved.HourlyRate);
        Assert.Null(saved.EmployeeInsuranceAmount);
    }

    [Fact]
    public async Task Create_UnknownJob_ReturnsRecordNotFound()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.JobId = 999;

        Assert.Equal("RecordNotFound", (await _sut.UpsertEmployeeAsync(employee, null, null)).Message);
        Assert.Empty(_db.Context.Employees);
    }

    [Fact]
    public async Task Create_WithoutAJob_ReturnsRecordNotFoundNotACrash()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.JobId = null;

        var result = await _sut.UpsertEmployeeAsync(employee, null, null);

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task Create_PhonesAreDistinctAndIncludeTheMainNumber()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.Phones = new List<EmployeePhone> { new() { PhoneNumber = "01000000001" }, new() { PhoneNumber = " 01222222222 " }, new() { PhoneNumber = "" } };

        await _sut.UpsertEmployeeAsync(employee, null, null);

        var phones = await _db.Context.EmployeePhones.AsNoTracking().Select(a => a.PhoneNumber).OrderBy(a => a).ToListAsync();
        Assert.Equal(new[] { "01000000001", "01222222222" }, phones);
    }

    [Fact]
    public async Task Create_AllowancesTakeNameAndAmountFromTheirType_NotFromTheForm()
    {
        var job = await SeedJob();
        var type = new AllowanceType { Code = "1", ArName = "نقل", EnName = "Transport", Amount = 300 };
        var deduction = new DeductionType { Code = "1", ArName = "جزاء", EnName = "Penalty", Amount = 50 };
        _db.Context.AddRange(type, deduction);
        await _db.Context.SaveChangesAsync();
        var employee = NewEmployee(job);
        employee.Allowances = new List<EmployeeAllowance>
        {
            new() { AllowanceTypeId = type.Id, Amount = 99999, ArName = "hacked", EnName = "hacked" },
            new() { AllowanceTypeId = null, Amount = 10, ArName = "typed by hand" }
        };
        employee.FixedDeductions = new List<EmployeeFixedDeduction> { new() { DeductionTypeId = deduction.Id, Amount = 1 } };

        await _sut.UpsertEmployeeAsync(employee, null, null);

        var allowance = await _db.Context.EmployeeAllowances.AsNoTracking().SingleAsync();
        Assert.Equal(300, allowance.Amount);
        Assert.Equal("Transport", allowance.EnName);
        Assert.Equal(50, (await _db.Context.EmployeeFixedDeductions.AsNoTracking().SingleAsync()).Amount);
    }

    [Fact]
    public async Task Create_LeaveDaysAreCountedInclusive()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.Leaves = new List<EmployeeLeave> { new() { FromDate = new DateTime(2030, 1, 1), ToDate = new DateTime(2030, 1, 5), Notes = "trip" } };

        await _sut.UpsertEmployeeAsync(employee, null, null);

        Assert.Equal(5, (await _db.Context.EmployeeLeaves.AsNoTracking().SingleAsync()).Days);
    }

    [Fact]
    public async Task Edit_RenamesEmployeeAndAccount_KeepsCodeAndRecalculatesRates()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        await _sut.UpsertEmployeeAsync(employee, null, null);
        _db.Context.ChangeTracker.Clear();

        var edit = NewEmployee(job, "Omar 2");
        edit.Id = employee.Id;
        edit.BasicSalary = 2600;
        var result = await _sut.UpsertEmployeeAsync(edit, null, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Employees.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.Equal("1", saved.Code);
        Assert.Equal("Omar 2", saved.Account.EnName);
        Assert.Equal(100, saved.DailyRate);
        Assert.Single(_db.Context.Accounts);
    }

    [Fact]
    public async Task Edit_AllowanceWithTheSameType_KeepsItsOldAmountEvenIfTheTypeChanged()
    {
        var job = await SeedJob();
        var type = new AllowanceType { Code = "1", ArName = "نقل", EnName = "Transport", Amount = 300 };
        _db.Context.AllowanceTypes.Add(type);
        await _db.Context.SaveChangesAsync();
        var employee = NewEmployee(job);
        employee.Allowances = new List<EmployeeAllowance> { new() { AllowanceTypeId = type.Id } };
        await _sut.UpsertEmployeeAsync(employee, null, null);
        var typeRow = await _db.Context.AllowanceTypes.SingleAsync();
        typeRow.Amount = 500;
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        var loaded = await _sut.GetEmployeeByIdAsync(employee.Id);

        var edit = NewEmployee(job);
        edit.Id = employee.Id;
        edit.Allowances = new List<EmployeeAllowance> { new() { Id = loaded.Allowances.Single().Id, AllowanceTypeId = type.Id, Amount = 1 } };
        _db.Context.ChangeTracker.Clear();
        var result = await _sut.UpsertEmployeeAsync(edit, null, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(300, (await _db.Context.EmployeeAllowances.AsNoTracking().SingleAsync()).Amount);
    }

    [Fact]
    public async Task Edit_ChangingTheJob_MovesTheLoginUserToTheNewRole()
    {
        var oldJob = await SeedJob("Cook");
        var newJob = await SeedJob("Waiter");
        var employee = NewEmployee(oldJob);
        await _sut.UpsertEmployeeAsync(employee, null, null);
        var cook = new IdentityRole { Name = "Cook", NormalizedName = "COOK" };
        var user = new ApplicationUser { UserName = "omar", Email = "o@test.com", Code = "1", UserType = "Cook" };
        _db.Context.Roles.Add(cook);
        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();
        _db.Context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = cook.Id });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var edit = NewEmployee(newJob);
        edit.Id = employee.Id;
        var result = await _sut.UpsertEmployeeAsync(edit, null, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var roles = await _db.Context.UserRoles.AsNoTracking().Where(a => a.UserId == user.Id).Join(_db.Context.Roles, a => a.RoleId, r => r.Id, (a, r) => r.Name).ToListAsync();
        Assert.Equal(new[] { "Waiter" }, roles);
        Assert.Equal("Waiter", (await _db.Context.Users.AsNoTracking().SingleAsync()).UserType);
    }

    [Fact]
    public async Task Edit_UnknownEmployee_ReturnsRecordNotFound()
    {
        var job = await SeedJob();
        var edit = NewEmployee(job);
        edit.Id = 404;

        Assert.Equal("RecordNotFound", (await _sut.UpsertEmployeeAsync(edit, null, null)).Message);
    }

    [Fact]
    public async Task Delete_RemovesTheEmployeeItsRowsAndItsAccount()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        employee.Leaves = new List<EmployeeLeave> { new() { FromDate = new DateTime(2030, 1, 1), ToDate = new DateTime(2030, 1, 2) } };
        employee.Phones = new List<EmployeePhone> { new() { PhoneNumber = "01222222222" } };
        await _sut.UpsertEmployeeAsync(employee, null, null);
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.DeleteEmployeeAsync(employee.Id);

        Assert.True(result.Success, result.Message);
        Assert.Empty(_db.Context.Employees);
        Assert.Empty(_db.Context.EmployeeLeaves);
        Assert.Empty(_db.Context.EmployeePhones);
        Assert.Empty(_db.Context.Accounts);
        Assert.True((await _sut.DeleteEmployeeAsync(404)).Success);
    }

    [Fact]
    public async Task Delete_EmployeeWithSalaryOrWithStores_IsBlocked()
    {
        var job = await SeedJob();
        var employee = NewEmployee(job);
        await _sut.UpsertEmployeeAsync(employee, null, null);
        var user = new ApplicationUser { UserName = "boss", Email = "b@test.com" };
        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();
        _db.Context.Salaries.Add(new Salary { EmployeeId = employee.Id, SalaryAmount = 10, UserId = user.Id, SalaryCode = "1" });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.DeleteEmployeeAsync(employee.Id);

        Assert.Equal("CannotDeleteLinkedRecord", result.Message);
        Assert.Single(_db.Context.Employees);
    }

    [Fact]
    public async Task Paging_SearchesByNameCodeAndJob_AndMarksEmployeesWithALogin()
    {
        var cook = await SeedJob("Cook");
        var waiter = await SeedJob("Waiter");
        await _sut.UpsertEmployeeAsync(NewEmployee(cook, "Omar"), null, null);
        await _sut.UpsertEmployeeAsync(NewEmployee(waiter, "Sara"), null, null);
        _db.Context.Users.Add(new ApplicationUser { UserName = "omar", Email = "o@test.com", Code = "1" });
        await _db.Context.SaveChangesAsync();

        var all = await _sut.GetPagedEmployeesAsync(new PageRequest());
        var byJob = await _sut.GetPagedEmployeesAsync(new PageRequest { SearchString = "waiter" });

        Assert.Equal(2, all.TotalRecords);
        Assert.True(all.Data.Single(a => a.Code == "1").HasLoginAccount);
        Assert.False(all.Data.Single(a => a.Code == "2").HasLoginAccount);
        Assert.Equal("Sara", Assert.Single(byJob.Data).EnName);
        Assert.Equal("3", await _sut.NewEmployeeCodeAsync());
    }
}
