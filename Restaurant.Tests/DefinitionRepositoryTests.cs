using System.Reflection;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// Runs the same CRUD contract against every five-method definition repository (GetPaged / GetById / Upsert / NewCode / Delete).
public class DefinitionRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    public static IEnumerable<object[]> Repositories() => new[]
    {
        new object[] { typeof(AllowanceTypeRepository), typeof(AllowanceType) },
        new object[] { typeof(ClassificationRepository), typeof(Classification) },
        new object[] { typeof(ContractTypeRepository), typeof(ContractType) },
        new object[] { typeof(CostCenterRepository), typeof(CostCenter) },
        new object[] { typeof(CurrencyRepository), typeof(Currency) },
        new object[] { typeof(CustomerTierRepository), typeof(CustomerTier) },
        new object[] { typeof(DeductionTypeRepository), typeof(DeductionType) },
        new object[] { typeof(DeliveryZoneRepository), typeof(DeliveryZone) },
        new object[] { typeof(DepartmentRepository), typeof(Department) },
        new object[] { typeof(ExpenseRepository), typeof(Expense) },
        new object[] { typeof(InsuranceCategoryRepository), typeof(InsuranceCategory) },
        new object[] { typeof(InsuranceCompanyRepository), typeof(InsuranceCompany) },
        new object[] { typeof(JobRepository), typeof(Job) },
        new object[] { typeof(JobTypeRepository), typeof(JobType) },
        new object[] { typeof(KitchenRepository), typeof(Kitchen) },
        new object[] { typeof(LeaveTypeRepository), typeof(LeaveType) },
        new object[] { typeof(MenuSectionRepository), typeof(MenuSection) },
        new object[] { typeof(ProductCategoryRepository), typeof(ProductCategory) },
        new object[] { typeof(SellTypeRepository), typeof(SellType) },
        new object[] { typeof(SpecializationRepository), typeof(Specialization) },
        new object[] { typeof(UnitPriceRepository), typeof(UnitPrice) },
        new object[] { typeof(TableRepository), typeof(Restaurant.DAL.Models.Table) },
        new object[] { typeof(CompanyRepository), typeof(Company) },
        new object[] { typeof(BranchRepository), typeof(CompanyBranch) },
        new object[] { typeof(StoreRepository), typeof(Store) },
    };

    private object Create(Type repoType)
    {
        var args = repoType.GetConstructors().Single().GetParameters()
            .Select(p => p.ParameterType == typeof(ApplicationDbContext) ? (object)_db.Context : _db.Http)
            .ToArray();
        return Activator.CreateInstance(repoType, args);
    }

    private static MethodInfo Find(object repo, string prefix, string suffix = "Async") =>
        repo.GetType().GetMethods().Single(m => m.Name.StartsWith(prefix) && m.Name.EndsWith(suffix) && !m.Name.Contains("Select"));

    private static async Task<object> Call(object repo, MethodInfo method, params object[] args)
    {
        var task = (Task)method.Invoke(repo, args);
        await task;
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static MethodInfo Get(object repo) => repo.GetType().GetMethods().Single(m => m.Name.StartsWith("Get") && m.Name.EndsWith("ByIdAsync"));
    private static MethodInfo Upsert(object repo) => repo.GetType().GetMethods().Single(m => m.Name.StartsWith("Upsert"));
    private static MethodInfo Delete(object repo) => repo.GetType().GetMethods().Single(m => m.Name.StartsWith("Delete"));
    private static MethodInfo Paged(object repo) => repo.GetType().GetMethods().Single(m => m.Name.StartsWith("GetPaged"));
    private static MethodInfo NewCode(object repo) => repo.GetType().GetMethods().SingleOrDefault(m => m.Name.StartsWith("New") && m.Name.EndsWith("CodeAsync"));

    private static T Make<T>(string ar, string en) where T : BaseEntity, new()
    {
        var entity = new T();
        entity.GetType().GetProperty("ArName").SetValue(entity, ar);
        entity.GetType().GetProperty("EnName").SetValue(entity, en);
        return entity;
    }

    private static object MakeEntity(Type entityType, string ar, string en)
    {
        var entity = Activator.CreateInstance(entityType);
        entityType.GetProperty("ArName").SetValue(entity, ar);
        entityType.GetProperty("EnName").SetValue(entity, en);
        return entity;
    }

    private static string CodeOf(object entity) => (string)entity.GetType().GetProperty("Code").GetValue(entity);
    private static int IdOf(object entity) => (int)entity.GetType().GetProperty("Id").GetValue(entity);
    private static void SetName(object entity, string en) => entity.GetType().GetProperty("EnName").SetValue(entity, en);

    private static void Assert_Success(object result, string context)
    {
        var success = (bool)result.GetType().GetProperty("Success").GetValue(result);
        var message = (string)result.GetType().GetProperty("Message").GetValue(result);
        Assert.True(success, $"{context}: {message}");
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Upsert_New_SavesWithCodeAndAudit(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");

        var result = await Call(repo, Upsert(repo), entity);

        Assert_Success(result, repoType.Name);
        var saved = (BaseEntity)await Call(repo, Get(repo), IdOf(entity));
        Assert.NotNull(saved);
        Assert.NotNull(saved.CreatedAt);
        Assert.False(string.IsNullOrWhiteSpace(CodeOf(saved)));
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Upsert_TwoNew_CodesAreDifferent(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var first = MakeEntity(entityType, "اول", "First");
        var second = MakeEntity(entityType, "ثاني", "Second");

        Assert_Success(await Call(repo, Upsert(repo), first), repoType.Name);
        Assert_Success(await Call(repo, Upsert(repo), second), repoType.Name);

        Assert.NotEqual(CodeOf(first), CodeOf(second));
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task NewCode_AfterInsert_ReturnsNextNumber(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var method = NewCode(repo);
        if (method is null) return;

        var before = (string)await Call(repo, method);
        Assert.Equal("1", before);

        var entity = MakeEntity(entityType, "اسم", "Name");
        Assert_Success(await Call(repo, Upsert(repo), entity), repoType.Name);

        var after = (string)await Call(repo, method);
        Assert.Equal("2", after);
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Upsert_Existing_UpdatesNameAndKeepsCodeAndCreatedAt(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");
        Assert_Success(await Call(repo, Upsert(repo), entity), repoType.Name);
        var original = (BaseEntity)await Call(repo, Get(repo), IdOf(entity));
        var code = CodeOf(original);
        var createdAt = original.CreatedAt;
        _db.Context.ChangeTracker.Clear();

        var edit = MakeEntity(entityType, "جديد", "Renamed");
        entityType.GetProperty("Id").SetValue(edit, IdOf(entity));
        // CostCenter, Currency and Specialization let the user edit the code on purpose; the rest ignore it.
        var codeEditable = new[] { typeof(CostCenter), typeof(Currency), typeof(Specialization) }.Contains(entityType);
        entityType.GetProperty("Code").SetValue(edit, codeEditable ? code : "HACKED");
        var result = await Call(repo, Upsert(repo), edit);

        Assert_Success(result, repoType.Name);
        _db.Context.ChangeTracker.Clear();
        var saved = (BaseEntity)await Call(repo, Get(repo), IdOf(entity));
        Assert.Equal("Renamed", (string)entityType.GetProperty("EnName").GetValue(saved));
        Assert.Equal(code, CodeOf(saved));
        Assert.Equal(createdAt, saved.CreatedAt);
        Assert.NotNull(saved.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Upsert_UnknownId_ReturnsRecordNotFound(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");
        entityType.GetProperty("Id").SetValue(entity, 9999);

        var result = await Call(repo, Upsert(repo), entity);

        Assert.False((bool)result.GetType().GetProperty("Success").GetValue(result));
        Assert.Equal("RecordNotFound", (string)result.GetType().GetProperty("Message").GetValue(result));
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task GetById_Unknown_ReturnsNull(Type repoType, Type entityType)
    {
        var repo = Create(repoType);

        Assert.Null(await Call(repo, Get(repo), 9999));
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Delete_Existing_RemovesRecord(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");
        Assert_Success(await Call(repo, Upsert(repo), entity), repoType.Name);

        var result = await Call(repo, Delete(repo), IdOf(entity));

        Assert_Success(result, repoType.Name);
        _db.Context.ChangeTracker.Clear();
        Assert.Null(await Call(repo, Get(repo), IdOf(entity)));
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Delete_Unknown_DoesNotThrow(Type repoType, Type entityType)
    {
        var repo = Create(repoType);

        var result = await Call(repo, Delete(repo), 9999);

        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task GetPaged_SearchAndPaging_FiltersAndCounts(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        foreach (var name in new[] { "Alpha", "Beta", "Gamma" })
            Assert_Success(await Call(repo, Upsert(repo), MakeEntity(entityType, name + "ar", name)), repoType.Name);

        var all = await Call(repo, Paged(repo), new PageRequest { PageSize = 10 });
        var filtered = await Call(repo, Paged(repo), new PageRequest { PageSize = 10, SearchString = "  bEtA " });
        var firstPage = await Call(repo, Paged(repo), new PageRequest { PageSize = 10, PageNo = 1 });

        Assert.Equal(3, (int)all.GetType().GetProperty("TotalRecords").GetValue(all));
        Assert.Equal(1, (int)filtered.GetType().GetProperty("TotalRecords").GetValue(filtered));
        Assert.Equal(3, ((System.Collections.IEnumerable)firstPage.GetType().GetProperty("Data").GetValue(firstPage)).Cast<object>().Count());
    }

    // ---------------------------------------------------------------- a failing database never escapes as an exception

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Upsert_WhenTheSaveFails_AnswersWithAFailureInsteadOfThrowing(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");
        _db.SaveFailure.Failure = new InvalidOperationException("forced");

        var result = await Call(repo, Upsert(repo), entity);

        Assert.False((bool)result.GetType().GetProperty("Success").GetValue(result), repoType.Name);
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Delete_WhenTheRowIsLinkedAndTheSaveFails_SaysItCannotBeDeleted(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");
        Assert_Success(await Call(repo, Upsert(repo), entity), repoType.Name);
        _db.SaveFailure.Failure = new Microsoft.EntityFrameworkCore.DbUpdateException("forced");

        var result = await Call(repo, Delete(repo), IdOf(entity));

        Assert.False((bool)result.GetType().GetProperty("Success").GetValue(result), repoType.Name);
    }

    [Theory]
    [MemberData(nameof(Repositories))]
    public async Task Delete_WhenTheDatabaseFailsInAnotherWay_AnswersWithAFailureInsteadOfThrowing(Type repoType, Type entityType)
    {
        var repo = Create(repoType);
        var entity = MakeEntity(entityType, "اسم", "Name");
        Assert_Success(await Call(repo, Upsert(repo), entity), repoType.Name);
        _db.SaveFailure.Failure = new InvalidOperationException("forced");

        var result = await Call(repo, Delete(repo), IdOf(entity));

        Assert.False((bool)result.GetType().GetProperty("Success").GetValue(result), repoType.Name);
    }
}
