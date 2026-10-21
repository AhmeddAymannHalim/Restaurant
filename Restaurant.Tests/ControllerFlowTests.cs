using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// The save / read / edit / delete journeys of the screens, through the real controller, service, validator-free model and database.
public class ControllerFlowTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    public static IEnumerable<object[]> Definitions() => DefinitionRepositoryTests.Repositories()
        .Select(a => new object[] { ((Type)a[0]).Name.Replace("Repository", string.Empty) });

    private static async Task<object> Run(object target, string name, Func<MethodInfo, bool> pick, params object[] args)
    {
        var method = target.GetType().GetMethods().Single(m => m.Name == name && pick(m));
        var result = method.Invoke(target, args);
        if (result is Task task)
        {
            await task;
            return task.GetType().GetProperty("Result").GetValue(task);
        }

        return result;
    }

    private static object Property(object json, string name) => ((JsonResult)json).Value.GetType().GetProperty(name).GetValue(((JsonResult)json).Value);

    [Theory]
    [MemberData(nameof(Definitions))]
    public async Task DefinitionScreen_SaveListOpenEditDelete_WorkEndToEnd(string name)
    {
        var type = typeof(ResturantPL.Controllers.Definitions.KitchenController).Assembly.GetTypes().Single(t => t.Name == name + "Controller");
        var controller = _host.Configure((Controller)_host.Get(type));
        var upsertPost = type.GetMethods().Single(m => m.Name == "Upsert" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType != typeof(int?));
        var model = Activator.CreateInstance(upsertPost.GetParameters()[0].ParameterType);
        model.GetType().GetProperty("ArName").SetValue(model, "اسم " + name);
        model.GetType().GetProperty("EnName").SetValue(model, "Name " + name);

        // save
        var saved = await Run(controller, "Upsert", m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType != typeof(int?), model);
        var redirect = Assert.IsType<RedirectToActionResult>(saved);
        Assert.Equal("Index", redirect.ActionName);
        _host.Toastr.Verify(t => t.Success("SavedSuccessfully"), Times.AtLeastOnce);

        // list
        var list = Assert.IsType<ViewResult>(await Run(controller, "Index", _ => true, new PageRequest { PageSize = 10 }));
        var total = (int)list.Model.GetType().GetProperty("TotalRecords").GetValue(list.Model);
        Assert.Equal(1, total);

        // open it
        _host.Context.ChangeTracker.Clear();
        var rows = ((System.Collections.IEnumerable)list.Model.GetType().GetProperty("Data").GetValue(list.Model)).Cast<object>().ToList();
        var id = (int)rows[0].GetType().GetProperty("Id").GetValue(rows[0]);
        var opened = Assert.IsType<ViewResult>(await Run(controller, "Upsert", m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int?), id));
        Assert.Equal("Name " + name, opened.Model.GetType().GetProperty("EnName").GetValue(opened.Model));

        // edit
        opened.Model.GetType().GetProperty("EnName").SetValue(opened.Model, "Renamed " + name);
        _host.Context.ChangeTracker.Clear();
        var edited = await Run(controller, "Upsert", m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType != typeof(int?), opened.Model);
        Assert.IsType<RedirectToActionResult>(edited);
        _host.Toastr.Verify(t => t.Success("UpdatedSuccessfully"), Times.AtLeastOnce);

        // delete (ajax json), twice is harmless
        _host.Context.ChangeTracker.Clear();
        Assert.Equal(true, Property(await Run(controller, "Delete", _ => true, id), "success"));
        Assert.Equal(true, Property(await Run(controller, "Delete", _ => true, id), "success"));
        _host.Context.ChangeTracker.Clear();
        var after = Assert.IsType<ViewResult>(await Run(controller, "Index", _ => true, new PageRequest { PageSize = 10 }));
        Assert.Equal(0, (int)after.Model.GetType().GetProperty("TotalRecords").GetValue(after.Model));
    }


    // ---------------------------------------------------------------- the export toolbar (Excel and PDF)

    private static TableExportRequest Request(params string[][] rows) => new()
    {
        Title = "Test / Sheet", FileName = "report", IsRightToLeft = true,
        Headings = new List<string> { "Code", "Name", "Amount" },
        Rows = rows.Select(r => r.ToList()).ToList()
    };

    [Fact]
    public void Excel_ReturnsAWorkbook_NumbersStayNumbers_CodesStayText_SheetNameIsCleaned()
    {
        var controller = _host.Configure(new ResturantPL.Controllers.TableExportController());

        var result = Assert.IsType<FileContentResult>(controller.Excel(Request(new[] { "001", "Pizza", "52.50" }, new[] { "002", "Cola", "1,200" }, new[] { "003", "Tea", "abc" })));

        Assert.Contains("spreadsheetml", result.ContentType);
        Assert.StartsWith("report-", result.FileDownloadName);
        Assert.EndsWith(".xlsx", result.FileDownloadName);
        using var book = new XLWorkbook(new MemoryStream(result.FileContents));
        var sheet = book.Worksheet(1);
        Assert.Equal("Test  Sheet", sheet.Name);
        Assert.True(sheet.RightToLeft);
        Assert.Equal("001", sheet.Cell(2, 1).GetString());
        Assert.Equal(XLDataType.Text, sheet.Cell(2, 1).DataType);
        Assert.Equal(52.5, sheet.Cell(2, 3).GetDouble());
        Assert.Equal(1200, sheet.Cell(3, 3).GetDouble());
        Assert.Equal("abc", sheet.Cell(4, 3).GetString());
        Assert.True(sheet.Cell(1, 1).Style.Font.Bold);
    }

    [Fact]
    public void Excel_And_Pdf_RefuseBadRequests()
    {
        var controller = _host.Configure(new ResturantPL.Controllers.TableExportController());
        var tooWide = new TableExportRequest { Title = "x", Headings = Enumerable.Range(0, 41).Select(i => "h" + i).ToList(), Rows = new List<List<string>>() };
        var tooLong = new TableExportRequest { Title = "x", Headings = new List<string> { "a" }, Rows = Enumerable.Range(0, 5001).Select(_ => new List<string> { "1" }).ToList() };

        Assert.IsType<BadRequestResult>(controller.Excel(null));
        Assert.IsType<BadRequestResult>(controller.Excel(new TableExportRequest { Title = "x", Headings = new List<string>(), Rows = new List<List<string>>() }));
        Assert.IsType<BadRequestResult>(controller.Excel(tooWide));
        Assert.IsType<BadRequestResult>(controller.Excel(tooLong));
        Assert.IsType<BadRequestResult>(controller.Pdf(null));
        Assert.IsType<BadRequestResult>(controller.Pdf(tooWide));
    }

    [Fact]
    public void Pdf_ForAValidRequest_ReturnsAPdfResultWithADatedFileName()
    {
        var controller = _host.Configure(new ResturantPL.Controllers.TableExportController());

        var result = controller.Pdf(Request(new[] { "001", "Pizza", "52.50" }));

        var pdf = Assert.IsType<Rotativa.AspNetCore.ViewAsPdf>(result);
        Assert.Equal("Pdf", pdf.ViewName);
        Assert.StartsWith("report-", Uri.UnescapeDataString(pdf.FileName));
        Assert.EndsWith(".pdf", pdf.FileName);
    }

    // ---------------------------------------------------------------- the shift screens

    private async Task SeedShift()
    {
        _host.Context.Users.Add(new ApplicationUser { Id = "user-1", UserName = "halim", Email = "h@test.com" });
        _host.Context.ApplicationSettings.Add(new ApplicationSetting { Password = "1234" });
        await _host.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task Shift_Login_WrongPasswordStaysAndComplains_RightPasswordOpensTheShift()
    {
        await SeedShift();
        var controller = _host.Controller<ResturantPL.Controllers.ShiftController>();

        Assert.IsType<ViewResult>(controller.Login());
        Assert.IsType<ViewResult>(await controller.Login("wrong"));
        _host.Toastr.Verify(t => t.Error("IncorrectShiftPassword"), Times.Once);
        Assert.Null(controller.HttpContext.Session.GetString("IsAuthenticated"));

        var ok = Assert.IsType<RedirectToActionResult>(await controller.Login("1234"));
        Assert.Equal("Dashboard", ok.ControllerName);
        Assert.Equal("true", controller.HttpContext.Session.GetString("IsAuthenticated"));
    }

    [Fact]
    public async Task Shift_End_NeedsThePassword_AsksAboutIncompleteOrders_AndClosesTheShift()
    {
        await SeedShift();
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        _host.Context.AddRange(store, section);
        await _host.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _host.Context.MenuItems.Add(dish);
        await _host.Context.SaveChangesAsync();
        var orders = new Restaurant.DAL.Repositories.Definitions.OrderRepository(_host.Context);
        Assert.True((await orders.CreateOrderAsync(new Order { Type = Shared.Enums.OrderType.TakeAway, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } }, "user-1")).Success);
        var controller = _host.Controller<ResturantPL.Controllers.ShiftController>();

        var wrong = Assert.IsType<RedirectToActionResult>(await controller.End("nope", 0));
        Assert.Equal("Order", wrong.ControllerName);

        var confirm = Assert.IsType<ViewResult>(await controller.End("1234", 40));
        Assert.Equal("ConfirmEnd", confirm.ViewName);
        Assert.Single(((Shared.ViewModels.Definitions.ShiftCloseConfirmViewModel)confirm.Model).IncompleteOrders);

        var done = Assert.IsType<RedirectToActionResult>(await controller.End("1234", 40, confirmOverride: true));
        Assert.Equal("Login", done.ActionName);
        Assert.Equal("false", controller.HttpContext.Session.GetString("IsAuthenticated"));
        Assert.True(await _host.Context.Orders.AllAsync(o => o.FinishShift));
        Assert.Single(_host.Context.ShiftCloseReports);
        _host.Toastr.Verify(t => t.Success("ShiftEnded"), Times.Once);

        var bad = Assert.IsType<RedirectToActionResult>(await controller.End("1234", -5, confirmOverride: true));
        Assert.Equal("Order", bad.ControllerName);
    }
}
