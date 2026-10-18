using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class EmployeeChildrenRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    // ---------- PrepareNew: a brand new employee ----------

    [Fact]
    public void PrepareNew_DropsBlankRowsAndResetsIds()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeChildrenRepository(context);
        var employee = new Employee
        {
            Code = "E1",
            ArName = "م",
            EnName = "Emp",
            Dependents = new List<EmployeeDependent>
            {
                new EmployeeDependent { Id = 5, ArName = "ابن", EnName = "Son" },
                new EmployeeDependent { Id = 6, ArName = "", EnName = "  " }
            }
        };

        // Act
        repository.PrepareNew(employee, null);

        // Assert
        var kept = Assert.Single(employee.Dependents);
        Assert.Equal("Son", kept.EnName);
        Assert.Equal(0, kept.Id);
        Assert.NotNull(kept.CreatedAt);
    }

    [Fact]
    public void PrepareNew_AllowanceWithOnlyAnAmount_IsKept_ButAnEmptyOneIsNot()
    {
        using var context = CreateContext();
        var repository = new EmployeeChildrenRepository(context);
        var employee = new Employee
        {
            Code = "E1",
            ArName = "م",
            EnName = "Emp",
            Allowances = new List<EmployeeAllowance>
            {
                new EmployeeAllowance { Amount = 100 },
                new EmployeeAllowance { Amount = 0 }
            }
        };

        repository.PrepareNew(employee, null);

        Assert.Equal(100, Assert.Single(employee.Allowances).Amount);
    }

    [Fact]
    public void PrepareNew_LeaveWithNothingFilled_IsDropped()
    {
        using var context = CreateContext();
        var repository = new EmployeeChildrenRepository(context);
        var employee = new Employee
        {
            Code = "E1",
            ArName = "م",
            EnName = "Emp",
            Leaves = new List<EmployeeLeave>
            {
                new EmployeeLeave(),
                new EmployeeLeave { Days = 3 }
            }
        };

        repository.PrepareNew(employee, null);

        Assert.Equal(3, Assert.Single(employee.Leaves).Days);
    }

    [Fact]
    public void PrepareNew_EmptyDocumentRow_IsDropped_ButOneWithANameIsKept()
    {
        using var context = CreateContext();
        var repository = new EmployeeChildrenRepository(context);
        var employee = new Employee
        {
            Code = "E1",
            ArName = "م",
            EnName = "Emp",
            Documents = new List<EmployeeDocument>
            {
                new EmployeeDocument(),
                new EmployeeDocument { Name = "Passport", Url = "/old/passport.pdf" }
            }
        };

        repository.PrepareNew(employee, null);

        var document = Assert.Single(employee.Documents);
        Assert.Equal("Passport", document.Name);
        Assert.Equal("/old/passport.pdf", document.Url);
    }

    [Fact]
    public void PrepareNew_NothingSent_LeavesEmptyLists()
    {
        using var context = CreateContext();
        var repository = new EmployeeChildrenRepository(context);
        var employee = new Employee { Dependents = null, Allowances = null, FixedDeductions = null, Leaves = null, Documents = null };

        repository.PrepareNew(employee, null);

        Assert.Empty(employee.Dependents);
        Assert.Empty(employee.Allowances);
        Assert.Empty(employee.FixedDeductions);
        Assert.Empty(employee.Leaves);
        Assert.Empty(employee.Documents);
    }

    // ---------- Sync: editing a saved employee ----------

    private static async Task<Employee> SaveEmployeeWithDependentsAsync(ApplicationDbContext context)
    {
        var employee = new Employee
        {
            Code = "E1",
            ArName = "م",
            EnName = "Emp",
            Dependents = new List<EmployeeDependent>
            {
                new EmployeeDependent { ArName = "ا", EnName = "Anna", Relation = "Wife" },
                new EmployeeDependent { ArName = "ب", EnName = "Ben", Relation = "Son" }
            }
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        return employee;
    }

    [Fact]
    public async Task Sync_ChangedRow_IsUpdatedInPlace()
    {
        // Arrange
        using var context = CreateContext();
        var saved = await SaveEmployeeWithDependentsAsync(context);
        var anna = saved.Dependents.First(a => a.EnName == "Anna");
        var ben = saved.Dependents.First(a => a.EnName == "Ben");
        var incoming = new Employee
        {
            Dependents = new List<EmployeeDependent>
            {
                new EmployeeDependent { Id = anna.Id, ArName = "ا", EnName = "Anna Smith", Relation = "Wife" },
                new EmployeeDependent { Id = ben.Id, ArName = "ب", EnName = "Ben", Relation = "Son" }
            }
        };
        var repository = new EmployeeChildrenRepository(context);

        // Act
        repository.Sync(saved, incoming, null);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(2, context.EmployeeDependents.Count());
        Assert.Equal("Anna Smith", context.EmployeeDependents.Single(a => a.Id == anna.Id).EnName);
        Assert.NotNull(context.EmployeeDependents.Single(a => a.Id == anna.Id).UpdatedAt);
    }

    [Fact]
    public async Task Sync_RowMissingFromTheScreen_IsDeleted()
    {
        using var context = CreateContext();
        var saved = await SaveEmployeeWithDependentsAsync(context);
        var anna = saved.Dependents.First(a => a.EnName == "Anna");
        var incoming = new Employee
        {
            Dependents = new List<EmployeeDependent> { new EmployeeDependent { Id = anna.Id, ArName = "ا", EnName = "Anna", Relation = "Wife" } }
        };
        var repository = new EmployeeChildrenRepository(context);

        repository.Sync(saved, incoming, null);
        await context.SaveChangesAsync();

        Assert.Equal("Anna", Assert.Single(context.EmployeeDependents).EnName);
    }

    [Fact]
    public async Task Sync_NewRowOnTheScreen_IsAdded()
    {
        using var context = CreateContext();
        var saved = await SaveEmployeeWithDependentsAsync(context);
        var keep = saved.Dependents.Select(a => new EmployeeDependent { Id = a.Id, ArName = a.ArName, EnName = a.EnName, Relation = a.Relation }).ToList();
        keep.Add(new EmployeeDependent { Id = 0, ArName = "ج", EnName = "Cara", Relation = "Daughter" });
        var repository = new EmployeeChildrenRepository(context);

        repository.Sync(saved, new Employee { Dependents = keep }, null);
        await context.SaveChangesAsync();

        Assert.Equal(3, context.EmployeeDependents.Count());
        Assert.Contains(context.EmployeeDependents, a => a.EnName == "Cara");
    }

    [Fact]
    public async Task Sync_BlankRowOnTheScreen_IsNotAdded_AndEmptyingTheListDeletesAll()
    {
        using var context = CreateContext();
        var saved = await SaveEmployeeWithDependentsAsync(context);
        var incoming = new Employee { Dependents = new List<EmployeeDependent> { new EmployeeDependent() } };
        var repository = new EmployeeChildrenRepository(context);

        repository.Sync(saved, incoming, null);
        await context.SaveChangesAsync();

        Assert.Empty(context.EmployeeDependents);
    }

    [Fact]
    public async Task Sync_LeaveRow_CopiesEveryField()
    {
        using var context = CreateContext();
        var employee = new Employee { Code = "E1", ArName = "م", EnName = "Emp", Leaves = new List<EmployeeLeave> { new EmployeeLeave { Days = 1, Notes = "old" } } };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var leaveId = employee.Leaves.Single().Id;
        var incoming = new Employee
        {
            Leaves = new List<EmployeeLeave>
            {
                new EmployeeLeave { Id = leaveId, FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 1, 5), Days = 5, Notes = "new", LeaveTypeId = 4 }
            }
        };
        var repository = new EmployeeChildrenRepository(context);

        repository.Sync(employee, incoming, null);
        await context.SaveChangesAsync();

        var leave = context.EmployeeLeaves.Single();
        Assert.Equal(5, leave.Days);
        Assert.Equal("new", leave.Notes);
        Assert.Equal(4, leave.LeaveTypeId);
        Assert.Equal(new DateTime(2026, 1, 5), leave.ToDate);
    }

    [Fact]
    public async Task Remove_DeletesEveryChildOfTheEmployee()
    {
        using var context = CreateContext();
        var employee = new Employee
        {
            Code = "E1",
            ArName = "م",
            EnName = "Emp",
            Dependents = new List<EmployeeDependent> { new EmployeeDependent { ArName = "ا", EnName = "Anna" } },
            Allowances = new List<EmployeeAllowance> { new EmployeeAllowance { Amount = 10 } },
            FixedDeductions = new List<EmployeeFixedDeduction> { new EmployeeFixedDeduction { Amount = 5 } },
            Leaves = new List<EmployeeLeave> { new EmployeeLeave { Days = 2 } },
            Documents = new List<EmployeeDocument> { new EmployeeDocument { Name = "ID" } }
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var repository = new EmployeeChildrenRepository(context);

        repository.Remove(employee);
        await context.SaveChangesAsync();

        Assert.Empty(context.EmployeeDependents);
        Assert.Empty(context.EmployeeAllowances);
        Assert.Empty(context.EmployeeFixedDeductions);
        Assert.Empty(context.EmployeeLeaves);
        Assert.Empty(context.EmployeeDocuments);
    }
}
