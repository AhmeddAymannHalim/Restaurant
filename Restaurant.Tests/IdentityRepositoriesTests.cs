using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Sign in / register, users, roles and the login account of an employee.
public class IdentityRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TestIdentity _identity;
    private readonly AccountRepository _accounts;
    private readonly UserRepository _users;
    private readonly RoleRepository _roles;
    private readonly EmployeeAccountRepository _employeeAccounts;

    public IdentityRepositoriesTests()
    {
        _identity = new TestIdentity(_db.Context);
        _accounts = new AccountRepository(_db.Context, _identity.Users, _identity.Roles, _identity.SignIn);
        _users = new UserRepository(_identity.Users, _identity.Roles);
        _roles = new RoleRepository(_identity.Roles, _identity.Users);
        _employeeAccounts = new EmployeeAccountRepository(_db.Context, _identity.Users, _identity.Roles);
    }

    public void Dispose()
    {
        _identity.Dispose();
        _db.Dispose();
    }

    private async Task Role(string name) => await _identity.Roles.CreateAsync(new IdentityRole(name));

    // a registered user keeps the Id its constructor made; the user screens send Id = null for a new user
    private static ApplicationUser Registered(string name) { var user = NewUser(name, "User"); user.Id = Guid.NewGuid().ToString(); return user; }

    private static ApplicationUser NewUser(string name, string type) => new() { Id = null, UserName = name, Email = name + "@test.com", FirstName = name, LastName = "L", UserType = type };

    // ---------------------------------------------------------------- account (register / sign in / passwords)

    [Fact]
    public async Task Register_CreatesTheUserRoleAndConfirmsTheEmail()
    {
        var result = await _accounts.RegisterCustomerAsync(Registered("ali"), "pass");

        Assert.True(result.Success, result.Message);
        var roles = await _accounts.GetRolesAsync(await _accounts.FindByEmailAsync("ali@test.com"));
        Assert.Equal(new[] { "User" }, roles);
        Assert.True(result.Data.EmailConfirmed);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsTheIdentityErrorKey()
    {
        await _accounts.RegisterCustomerAsync(Registered("ali"), "pass");
        var again = Registered("ali2");
        again.Email = "ali@test.com";

        var result = await _accounts.RegisterCustomerAsync(again, "pass");

        Assert.False(result.Success);
        Assert.Equal("Identity.DuplicateEmail", result.Message);
    }

    [Fact]
    public async Task CheckPassword_RightWrongUnknownAndLockout()
    {
        await _accounts.RegisterCustomerAsync(Registered("ali"), "pass");

        Assert.True((await _accounts.CheckPasswordAsync("ali@test.com", "pass")).Success);
        Assert.Equal("Auth_InvalidLogin", (await _accounts.CheckPasswordAsync("ali@test.com", "wrong")).Message);
        Assert.Equal("Auth_InvalidLogin", (await _accounts.CheckPasswordAsync("nobody@test.com", "pass")).Message);

        for (var i = 0; i < 5; i++)
        {
            await _accounts.CheckPasswordAsync("ali@test.com", "wrong");
        }

        Assert.Equal("Auth_LockedOut", (await _accounts.CheckPasswordAsync("ali@test.com", "pass")).Message);
    }

    [Fact]
    public async Task ChangePassword_NeedsTheCurrentOne()
    {
        var user = (await _accounts.RegisterCustomerAsync(Registered("ali"), "pass")).Data;

        Assert.Equal("Auth_WrongPassword", (await _accounts.ChangePasswordAsync(user.Id, "nope", "new")).Message);
        Assert.True((await _accounts.ChangePasswordAsync(user.Id, "pass", "new")).Success);
        Assert.True((await _accounts.CheckPasswordAsync("ali@test.com", "new")).Success);
        Assert.Equal("RecordNotFound", (await _accounts.ChangePasswordAsync("ghost", "a", "b")).Message);
    }

    [Fact]
    public async Task ResetPassword_WithAValidToken_ClearsTheLockout()
    {
        var user = (await _accounts.RegisterCustomerAsync(Registered("ali"), "pass")).Data;
        for (var i = 0; i < 5; i++)
        {
            await _accounts.CheckPasswordAsync("ali@test.com", "wrong");
        }
        var token = await _accounts.GeneratePasswordResetTokenAsync(user);

        Assert.Equal("RecordNotFound", (await _accounts.ResetPasswordAsync("nobody@test.com", token, "x")).Message);
        Assert.False((await _accounts.ResetPasswordAsync("ali@test.com", "bad-token", "x")).Success);
        var ok = await _accounts.ResetPasswordAsync("ali@test.com", token, "fresh");

        Assert.True(ok.Success, ok.Message);
        Assert.True((await _accounts.CheckPasswordAsync("ali@test.com", "fresh")).Success);
    }

    [Fact]
    public async Task ConfirmEmail_UnknownUserAndBadToken()
    {
        var user = new ApplicationUser { UserName = "x", Email = "x@test.com" };
        await _identity.Users.CreateAsync(user, "pass");
        var token = await _accounts.GenerateConfirmationTokenAsync(user);

        Assert.Equal("RecordNotFound", (await _accounts.ConfirmEmailAsync("ghost", token)).Message);
        Assert.False((await _accounts.ConfirmEmailAsync(user.Id, "bad")).Success);
        Assert.True((await _accounts.ConfirmEmailAsync(user.Id, token)).Success);
        Assert.NotNull(await _accounts.GetByIdAsync(user.Id));
    }

    // ---------------------------------------------------------------- users

    [Fact]
    public async Task User_Create_NeedsAnExistingRole_AndGetsIt()
    {
        Assert.Equal("RoleNotFound", (await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass")).Message);

        await Role("Waiter");
        var result = await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass");

        Assert.True(result.Success, result.Message);
        var saved = await _identity.Users.FindByNameAsync("sara");
        Assert.True(saved.EmailConfirmed);
        Assert.Equal(new[] { "Waiter" }, await _identity.Users.GetRolesAsync(saved));
    }

    [Fact]
    public async Task User_Edit_ChangesFieldsAndMovesTheRole()
    {
        await Role("Waiter");
        await Role("Cook");
        await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass");
        var saved = await _identity.Users.FindByNameAsync("sara");

        var edit = NewUser("sara2", "Cook");
        edit.Id = saved.Id;
        var result = await _users.UpsertUserAsync(edit, null);

        Assert.True(result.Success, result.Message);
        var after = await _identity.Users.FindByIdAsync(saved.Id);
        Assert.Equal("sara2", after.UserName);
        Assert.Equal(new[] { "Cook" }, await _identity.Users.GetRolesAsync(after));
        Assert.Equal("RecordNotFound", (await _users.UpsertUserAsync(new ApplicationUser { Id = "ghost", UserType = "Cook" }, null)).Message);
    }

    [Fact]
    public async Task User_TheLastAdmin_CannotBeDemotedOrDeleted()
    {
        await Role("Admin");
        await Role("Waiter");
        await _users.UpsertUserAsync(NewUser("boss", "Admin"), "pass");
        var boss = await _identity.Users.FindByNameAsync("boss");
        var demote = NewUser("boss", "Waiter");
        demote.Id = boss.Id;

        Assert.Equal("LastAdminCannotChange", (await _users.UpsertUserAsync(demote, null)).Message);
        Assert.Equal("LastAdminCannotChange", (await _users.DeleteUserAsync(boss.Id)).Message);

        await _users.UpsertUserAsync(NewUser("boss2", "Admin"), "pass");
        Assert.True((await _users.DeleteUserAsync(boss.Id)).Success);
        Assert.Equal("RecordNotFound", (await _users.DeleteUserAsync("ghost")).Message);
    }

    [Fact]
    public async Task User_WhoMadeOrders_CanNotBeDeletedButGetsAMessageNotACrash()
    {
        await Role("Waiter");
        await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass");
        var sara = await _identity.Users.FindByNameAsync("sara");
        _db.Context.Orders.Add(new Order { OrderCode = "1", SellerId = sara.Id, Bill = new BillMaster { Code = "1", Date = DateTime.Today } });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var result = await _users.DeleteUserAsync(sara.Id);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Message));
    }

    [Fact]
    public async Task User_ExistChecks_AndPaging()
    {
        await Role("Waiter");
        await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass");
        var sara = await _identity.Users.FindByNameAsync("sara");

        Assert.True(_users.IsEmailExist(null, " SARA@test.com "));
        Assert.False(_users.IsEmailExist(sara.Id, "sara@test.com"));
        Assert.True(_users.IsUserNameExist("other", "Sara"));
        Assert.False(_users.IsUserNameExist(null, ""));
        Assert.Single((await _users.GetPagedUsersAsync(new PageRequest { SearchString = "waiter" })).Data);
        Assert.NotNull(await _users.GetUserByIdAsync(sara.Id));
    }

    // ---------------------------------------------------------------- roles

    [Fact]
    public async Task Role_CreateRenameDelete_AndSystemRolesAreLocked()
    {
        Assert.True((await _roles.UpsertRoleAsync(new IdentityRole("Waiter") { Id = null })).Success);
        var role = await _identity.Roles.FindByNameAsync("Waiter");
        Assert.True((await _roles.UpsertRoleAsync(new IdentityRole("Waiter") { Id = role.Id, Name = "Server" })).Success);
        Assert.Equal("Server", (await _roles.GetRoleByIdAsync(role.Id)).Name);
        Assert.Equal("RecordNotFound", (await _roles.UpsertRoleAsync(new IdentityRole("x") { Id = "ghost" })).Message);

        await Role("Admin");
        var admin = await _identity.Roles.FindByNameAsync("Admin");
        Assert.Equal("SystemRoleLocked", (await _roles.UpsertRoleAsync(new IdentityRole("Boss") { Id = admin.Id })).Message);
        Assert.Equal("SystemRoleLocked", (await _roles.DeleteRoleAsync(admin.Id)).Message);

        Assert.True((await _roles.DeleteRoleAsync(role.Id)).Success);
        Assert.Equal("RecordNotFound", (await _roles.DeleteRoleAsync("ghost")).Message);
        Assert.True(_roles.IsNameExist(null, "admin"));
        Assert.False(_roles.IsNameExist(admin.Id, "admin"));
    }

    [Fact]
    public async Task Role_WithUsers_CannotBeDeleted()
    {
        await Role("Waiter");
        await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass");
        var role = await _identity.Roles.FindByNameAsync("Waiter");

        var result = await _roles.DeleteRoleAsync(role.Id);

        Assert.False(result.Success);
        Assert.NotNull(await _identity.Roles.FindByNameAsync("Waiter"));
    }

    [Fact]
    public async Task Role_Renamed_KeepsItsUsersInSync()
    {
        await Role("Waiter");
        await _users.UpsertUserAsync(NewUser("sara", "Waiter"), "pass");
        var role = await _identity.Roles.FindByNameAsync("Waiter");

        await _roles.UpsertRoleAsync(new IdentityRole("Server") { Id = role.Id });

        var sara = await _identity.Users.FindByNameAsync("sara");
        Assert.Equal("Server", sara.UserType);
        Assert.Equal(new[] { "Server" }, await _identity.Users.GetRolesAsync(sara));
    }

    // ---------------------------------------------------------------- employee login accounts

    private async Task<Employee> SeedEmployee(string jobName = "Cook", string code = "1")
    {
        var job = jobName == null ? null : new Job { Code = code, ArName = jobName, EnName = jobName };
        var employee = new Employee { Code = code, ArName = "Omar", EnName = "Omar", Job = job, PhoneNumber = "010", Address = "St" };
        _db.Context.Employees.Add(employee);
        await _db.Context.SaveChangesAsync();
        return employee;
    }

    [Fact]
    public async Task EmployeeAccount_Create_BuildsALoginInTheRoleOfTheJob()
    {
        var employee = await SeedEmployee();

        var result = await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "omar", Email = "omar@test.com" }, employee.Id, "pass");

        Assert.True(result.Success, result.Message);
        var user = await _identity.Users.FindByNameAsync("omar");
        Assert.Equal("1", user.Code);
        Assert.Equal("Cook", user.UserType);
        Assert.Equal(new[] { "Cook" }, await _identity.Users.GetRolesAsync(user));
        Assert.Empty(await _employeeAccounts.GetEmployeesWithoutAccountAsync());
        Assert.Equal(user.Id, (await _employeeAccounts.GetAccountByEmployeeIdAsync(employee.Id)).Id);
        Assert.Single((await _employeeAccounts.GetPagedAccountsAsync(new PageRequest())).Data);
    }

    [Fact]
    public async Task EmployeeAccount_UnknownEmployee_ReturnsRecordNotFound()
    {
        var result = await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "x", Email = "x@test.com" }, 404, "pass");

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task EmployeeAccount_EmployeeWithoutAJob_ReturnsAMessageNotACrash()
    {
        var employee = await SeedEmployee(jobName: null, code: "7");

        var result = await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "x", Email = "x@test.com" }, employee.Id, "pass");

        Assert.False(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Message));
    }

    [Fact]
    public async Task EmployeeAccount_SecondLoginForTheSameEmployee_IsRejected()
    {
        var employee = await SeedEmployee();
        await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "omar", Email = "omar@test.com" }, employee.Id, "pass");

        var again = await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "omar2", Email = "omar2@test.com" }, employee.Id, "pass");

        Assert.False(again.Success);
        Assert.Equal(1, await _identity.Users.Users.CountAsync(a => a.Code == "1"));
    }

    [Fact]
    public async Task EmployeeAccount_Edit_ChangesLoginAndPassword()
    {
        var employee = await SeedEmployee();
        await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "omar", Email = "omar@test.com" }, employee.Id, "pass");
        var user = await _identity.Users.FindByNameAsync("omar");

        var result = await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = user.Id, UserName = "omar9", Email = "o9@test.com" }, employee.Id, "newpass");

        Assert.True(result.Success, result.Message);
        Assert.True((await _accounts.CheckPasswordAsync("o9@test.com", "newpass")).Success);
        Assert.Equal("RecordNotFound", (await _employeeAccounts.UpsertAccountAsync(new ApplicationUser { Id = "ghost" }, employee.Id, null)).Message);
    }
}
