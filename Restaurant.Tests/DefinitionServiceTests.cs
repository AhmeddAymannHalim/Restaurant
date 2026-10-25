using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Each definition service goes through the real mapper, the real unit of work and the database:
// a view model saved through the service must come back unchanged, and an edit by Id must stick.
public class DefinitionServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public DefinitionServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Restaurant.BLL.Helper.AutoMapper>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
        _unitOfWork = new UnitOfWork(_db.Context, _db.Http);
    }

    public void Dispose() => _db.Dispose();

    public static IEnumerable<object[]> Services() => DefinitionRepositoryTests.Repositories()
        .Select(a => new object[] { ((Type)a[0]).Name.Replace("Repository", "Service") });

    private object Create(string serviceName)
    {
        var type = typeof(Restaurant.BLL.Services.Definitions.KitchenService).Assembly.GetTypes().Single(a => a.Name == serviceName);
        return Activator.CreateInstance(type, _unitOfWork, _mapper);
    }

    private static System.Reflection.MethodInfo Find(object service, string prefix, string suffix = "Async") =>
        service.GetType().GetMethods().Single(m => m.Name.StartsWith(prefix) && m.Name.EndsWith(suffix));

    private static async Task<object> Call(object service, System.Reflection.MethodInfo method, params object[] args)
    {
        var task = (Task)method.Invoke(service, args);
        await task;
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static object Model(System.Reflection.MethodInfo upsert, string ar, string en, int id = 0)
    {
        var model = Activator.CreateInstance(upsert.GetParameters()[0].ParameterType);
        model.GetType().GetProperty("ArName").SetValue(model, ar);
        model.GetType().GetProperty("EnName").SetValue(model, en);
        model.GetType().GetProperty("Id")?.SetValue(model, id);
        return model;
    }

    private static T Get<T>(object target, string property) => (T)target.GetType().GetProperty(property).GetValue(target);

    [Theory]
    [MemberData(nameof(Services))]
    public async Task Save_Read_Edit_Delete_GoThroughTheMapperAndKeepTheValues(string serviceName)
    {
        var service = Create(serviceName);
        var upsert = Find(service, "Upsert");

        var created = await Call(service, upsert, Model(upsert, "اسم", "Name"));
        Assert.True(Get<bool>(created, "Success"), serviceName + ": " + Get<string>(created, "Message"));

        var page = await Call(service, Find(service, "GetPaged"), new PageRequest { PageSize = 10 });
        Assert.Equal(1, Get<int>(page, "TotalRecords"));
        var row = ((System.Collections.IEnumerable)Get<object>(page, "Data")).Cast<object>().Single();
        Assert.Equal("اسم", Get<string>(row, "ArName"));
        Assert.Equal("Name", Get<string>(row, "EnName"));
        var id = Get<int>(row, "Id");
        Assert.True(id > 0);

        _db.Context.ChangeTracker.Clear();
        var loaded = await Call(service, Find(service, "Get", "ByIdAsync"), id);
        Assert.Equal("Name", Get<string>(loaded, "EnName"));
        if (loaded.GetType().GetProperty("Code") is not null)
        {
            Assert.False(string.IsNullOrWhiteSpace(Get<string>(loaded, "Code")));
        }

        var edited = await Call(service, upsert, Model(upsert, "جديد", "Renamed", id));
        Assert.True(Get<bool>(edited, "Success"), serviceName + ": " + Get<string>(edited, "Message"));
        _db.Context.ChangeTracker.Clear();
        Assert.Equal("Renamed", Get<string>(await Call(service, Find(service, "Get", "ByIdAsync"), id), "EnName"));

        var deleted = await Call(service, Find(service, "Delete"), id);
        Assert.True(Get<bool>(deleted, "Success"));
        _db.Context.ChangeTracker.Clear();
        var after = await Call(service, Find(service, "GetPaged"), new PageRequest { PageSize = 10 });
        Assert.Equal(0, Get<int>(after, "TotalRecords"));
    }

    [Theory]
    [MemberData(nameof(Services))]
    public async Task GetById_Unknown_ReturnsNullNotACrash(string serviceName)
    {
        var service = Create(serviceName);

        Assert.Null(await Call(service, Find(service, "Get", "ByIdAsync"), 9999));
    }
}

// A refund asked on the supplier screen must land on the supplier payment, not the customer one.
public class PaymentRefundServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task SupplierRefund_RequestedFromTheForm_IsSavedOnTheSupplierPayment()
    {
        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<Restaurant.BLL.Helper.AutoMapper>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        var unitOfWork = new Restaurant.DAL.UnitOfWork.UnitOfWork(_db.Context, _db.Http);
        var service = new Restaurant.BLL.Services.Definitions.SupplierPaymentService(unitOfWork, config.CreateMapper());
        var supplier = new Restaurant.DAL.Models.Supplier { Account = new Restaurant.DAL.Models.Account { Code = "S", ArName = "م", EnName = "Sup", AccountType = Shared.Enums.AccountType.Supplier, BalanceType = Shared.Enums.AccountBalanceType.Credit } };
        var user = new Restaurant.DAL.Models.ApplicationUser { UserName = "u", Email = "u@test.com" };
        _db.Context.AddRange(supplier, user);
        await _db.Context.SaveChangesAsync();
        var payment = new Restaurant.DAL.Models.PaymentInvoiceSupplier { SupplierId = supplier.Id, ProcessCode = "1", Paid = 100, Date = DateTime.Now };
        _db.Context.PaymentInvoiceSupplier.Add(payment);
        await _db.Context.SaveChangesAsync();

        var result = await service.RequestRefundAsync(new Shared.ViewModels.Definitions.PaymentRefundRequestViewModel { OriginalPaymentId = payment.Id, Amount = 40, Reason = "back" }, user.Id);

        Assert.True(result.Success, result.Message);
        var refund = _db.Context.PaymentRefunds.Single();
        Assert.Equal(payment.Id, refund.SupplierPaymentId);
        Assert.Null(refund.OriginalPaymentId);
        Assert.Equal(40, refund.Amount);
    }
}
