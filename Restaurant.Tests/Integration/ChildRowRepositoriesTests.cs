using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.Tests;

// Phone lists and the child rows of an employee (dependents, allowances, deductions, leaves, documents).
[Collection("global-state")]
public class ChildRowRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly string _oldFolder = Environment.CurrentDirectory;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "child-rows-" + Guid.NewGuid().ToString("N"));

    public ChildRowRepositoriesTests()
    {
        Directory.CreateDirectory(_folder);
        Environment.CurrentDirectory = _folder;
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _oldFolder;
        _db.Dispose();
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // a locked temp file is not a test failure
        }
    }

    private static IFormFile File(string name, string text = "x")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "f", name);
    }

    // ---------------------------------------------------------------- phones

    [Fact]
    public void CleanNumbers_DropsBlanks_TheMainNumber_AndRepeats()
    {
        var sut = new PhoneRepository(_db.Context);
        var rows = new List<SupplierPhone> { new() { PhoneNumber = " 0100 " }, new() { PhoneNumber = "0100" }, new() { PhoneNumber = "" }, new() { PhoneNumber = null }, new() { PhoneNumber = "0111" }, new() { PhoneNumber = "0122" } };

        Assert.Equal(new[] { "0100", "0122" }, sut.CleanNumbers("0111", rows));
        Assert.Empty(sut.CleanNumbers("0111", null));
    }

    [Fact]
    public async Task SyncSupplierPhones_RemoveOldAddNewKeepSame()
    {
        var sut = new PhoneRepository(_db.Context);
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "s", EnName = "s", AccountType = Shared.Enums.AccountType.Supplier }, Phones = new List<SupplierPhone> { new() { PhoneNumber = "1" }, new() { PhoneNumber = "2" } } };
        _db.Context.Add(supplier);
        await _db.Context.SaveChangesAsync();

        sut.SyncSupplierPhones(supplier.Phones, new List<string> { "2", "3" });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        Assert.Equal(new[] { "2", "3" }, _db.Context.Set<SupplierPhone>().Select(a => a.PhoneNumber).OrderBy(a => a).ToArray());
    }

    // ---------------------------------------------------------------- employee child rows

    private static Employee Incoming() => new()
    {
        Dependents = new List<EmployeeDependent> { new() { ArName = "ابن", EnName = "Son", Relation = "Son" }, new() { ArName = "", EnName = "" } },
        Allowances = new List<EmployeeAllowance> { new() { ArName = "بدل", EnName = "Transport", Amount = 200 }, new() { Amount = 0 } },
        FixedDeductions = new List<EmployeeFixedDeduction> { new() { ArName = "خصم", EnName = "Loan", Amount = 50 }, new() { Amount = 0 } },
        Leaves = new List<EmployeeLeave> { new() { FromDate = new DateTime(2030, 1, 1), ToDate = new DateTime(2030, 1, 3), Days = 3 }, new() },
        Documents = new List<EmployeeDocument> { new() { Name = "Contract" }, new() { Name = "" } }
    };

    [Fact]
    public async Task PrepareNew_KeepsOnlyFilledRows_AsNewRecords_AndUploadsDocuments()
    {
        var sut = new EmployeeChildrenRepository(_db.Context);
        var employee = Incoming();
        employee.Dependents.First().Id = 77;

        sut.PrepareNew(employee, new List<IFormFile> { File("contract.pdf", "pdf") });

        Assert.Single(employee.Dependents);
        Assert.Equal(0, employee.Dependents.First().Id);
        Assert.Single(employee.Allowances);
        Assert.Single(employee.FixedDeductions);
        Assert.Single(employee.Leaves);
        var document = Assert.Single(employee.Documents);
        Assert.StartsWith("/Employee Documents/", document.Url);
        Assert.True(System.IO.File.Exists(Path.Combine(_folder, "wwwroot", document.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
        await Task.CompletedTask;
    }

    private async Task<Employee> SavedEmployee()
    {
        var job = new Job { Code = "1", ArName = "J", EnName = "J" };
        var employee = new Employee { Code = "1", ArName = "Emp", EnName = "Emp", Job = job };
        new EmployeeChildrenRepository(_db.Context).PrepareNew(employee = Merge(employee), new List<IFormFile> { File("first.pdf") });
        _db.Context.Add(employee);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        return await _db.Context.Employees.Include(a => a.Dependents).Include(a => a.Allowances).Include(a => a.FixedDeductions).Include(a => a.Leaves).Include(a => a.Documents).SingleAsync();
    }

    private static Employee Merge(Employee employee)
    {
        var rows = Incoming();
        employee.Dependents = rows.Dependents;
        employee.Allowances = rows.Allowances;
        employee.FixedDeductions = rows.FixedDeductions;
        employee.Leaves = rows.Leaves;
        employee.Documents = rows.Documents;

        return employee;
    }

    [Fact]
    public async Task Sync_UpdatesOldRows_AddsNewRows_RemovesMissingRows_AndReplacesFiles()
    {
        var saved = await SavedEmployee();
        var oldUrl = saved.Documents.Single().Url;
        var incoming = new Employee
        {
            Dependents = new List<EmployeeDependent> { new() { Id = saved.Dependents.Single().Id, ArName = "ابنة", EnName = "Daughter", Relation = "Daughter" }, new() { ArName = "آخر", EnName = "Other" } },
            Allowances = new List<EmployeeAllowance>(),
            FixedDeductions = new List<EmployeeFixedDeduction> { new() { Id = saved.FixedDeductions.Single().Id, ArName = "x", EnName = "Loan 2", Amount = 75 } },
            Leaves = new List<EmployeeLeave> { new() { Id = saved.Leaves.Single().Id, FromDate = new DateTime(2030, 2, 1), ToDate = new DateTime(2030, 2, 2), Days = 2, Notes = "moved" }, new() { FromDate = new DateTime(2030, 3, 1), ToDate = new DateTime(2030, 3, 1), Days = 1 } },
            Documents = new List<EmployeeDocument> { new() { Id = saved.Documents.Single().Id, Name = "Renamed" }, new() { Name = "Passport" } }
        };

        new EmployeeChildrenRepository(_db.Context).Sync(saved, incoming, new List<IFormFile> { File("second.pdf", "new"), null });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var after = await _db.Context.Employees.Include(a => a.Dependents).Include(a => a.Allowances).Include(a => a.FixedDeductions).Include(a => a.Leaves).Include(a => a.Documents).SingleAsync();
        Assert.Equal(new[] { "Daughter", "Other" }, after.Dependents.Select(a => a.EnName).OrderBy(a => a).ToArray());
        Assert.Empty(after.Allowances);
        Assert.Equal(75, after.FixedDeductions.Single().Amount);
        Assert.Equal(2, after.Leaves.Count);
        Assert.Contains(after.Leaves, a => a.Notes == "moved");
        Assert.Equal(2, after.Documents.Count);
        var renamed = after.Documents.Single(a => a.Name == "Renamed");
        Assert.EndsWith("_second.pdf", renamed.Url);
        Assert.NotEqual(oldUrl, renamed.Url);
        Assert.False(System.IO.File.Exists(Path.Combine(_folder, "wwwroot", oldUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task Sync_EmptyIncoming_RemovesEveryRow_AndTheirFiles()
    {
        var saved = await SavedEmployee();
        var url = saved.Documents.Single().Url;

        new EmployeeChildrenRepository(_db.Context).Sync(saved, new Employee(), null);
        await _db.Context.SaveChangesAsync();

        Assert.Empty(_db.Context.EmployeeDependents);
        Assert.Empty(_db.Context.EmployeeAllowances);
        Assert.Empty(_db.Context.EmployeeFixedDeductions);
        Assert.Empty(_db.Context.EmployeeLeaves);
        Assert.Empty(_db.Context.EmployeeDocuments);
        Assert.False(System.IO.File.Exists(Path.Combine(_folder, "wwwroot", url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task Remove_DeletesEveryChildRow_AndTheUploadedFiles()
    {
        var saved = await SavedEmployee();
        var url = saved.Documents.Single().Url;

        new EmployeeChildrenRepository(_db.Context).Remove(saved);
        await _db.Context.SaveChangesAsync();

        Assert.Empty(_db.Context.EmployeeDependents);
        Assert.Empty(_db.Context.EmployeeDocuments);
        Assert.False(System.IO.File.Exists(Path.Combine(_folder, "wwwroot", url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
    }
}
