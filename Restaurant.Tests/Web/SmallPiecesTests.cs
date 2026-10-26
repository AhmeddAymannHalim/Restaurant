using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using ResturantPL.Controllers;
using ResturantPL.Controllers.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The small pieces that had no direct test: language switch, report and stock movement screens, the generic duplicate checks,
// a cashier order started from a reservation, the phone rules and the asset rules of the account form.
public class SmallPiecesTests : IDisposable
{
    private const string ErrorView = "~/Views/Shared/Error.cshtml";

    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    // ---------------------------------------------------------------- language switch

    private static SetLanguageController Language(out DefaultHttpContext http)
    {
        http = new DefaultHttpContext();
        var url = new Mock<IUrlHelper>();
        url.Setup(a => a.IsLocalUrl(It.IsAny<string>())).Returns((string value) => value is not null && value.StartsWith("/"));

        return new SetLanguageController
        {
            ControllerContext = new ControllerContext { HttpContext = http },
            Url = url.Object
        };
    }

    [Theory]
    [InlineData("ar", true)]
    [InlineData("en", true)]
    [InlineData("fr", false)]
    [InlineData(null, false)]
    public void SetLanguage_OnlyArabicAndEnglishAreRemembered(string culture, bool remembered)
    {
        var controller = Language(out var http);

        controller.SetLanguage(culture, "/Dashboard");

        Assert.Equal(remembered, http.Response.Headers.SetCookie.ToString().Contains(CookieRequestCultureProvider.DefaultCookieName));
    }

    [Theory]
    [InlineData("/Order/Upsert", "/Order/Upsert")]
    [InlineData("https://evil.example/", "~/")]
    [InlineData(null, "~/")]
    public void SetLanguage_GoesBackOnlyToALocalPage(string returnUrl, string goesTo)
    {
        var controller = Language(out _);

        var result = controller.SetLanguage("ar", returnUrl);

        Assert.Equal(goesTo, Assert.IsType<LocalRedirectResult>(result).Url);
    }

    // ---------------------------------------------------------------- reports

    [Fact]
    public async Task Report_Index_ListsTheReports()
    {
        var controller = new ReportController(new Mock<IReportService>().Object);

        var names = Assert.IsType<string[]>(Assert.IsType<ViewResult>(controller.Index()).Model);

        Assert.Contains("MoneySafe", names);
        Assert.Contains("Scrounges", names);
        Assert.Equal(14, names.Length);
    }

    [Fact]
    public async Task Report_Show_AnUnknownOrMissingReportGoesBackToTheList_AKnownOneIsBuilt_AFailureEndsOnTheErrorPage()
    {
        var service = new Mock<IReportService>(MockBehavior.Loose);
        service.Setup(a => a.BuildReportAsync(It.Is<ReportRequest>(r => r.Report == "Profit"))).ReturnsAsync(new ReportViewModel());
        service.Setup(a => a.BuildReportAsync(It.Is<ReportRequest>(r => r.Report == "Orders"))).ThrowsAsync(new InvalidOperationException());
        var controller = new ReportController(service.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await controller.Show(null)).ActionName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await controller.Show(new ReportRequest { Report = "NoSuchReport" })).ActionName);
        Assert.IsType<ViewResult>(await controller.Show(new ReportRequest { Report = "Profit" }));
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await controller.Show(new ReportRequest { Report = "Orders" })).ViewName);
    }

    // ---------------------------------------------------------------- stock movements

    [Fact]
    public async Task StockMovement_Index_FiltersByStoreAndProduct_AndAFailureEndsOnTheErrorPage()
    {
        var service = new Mock<IStockMovementService>(MockBehavior.Loose);
        service.Setup(a => a.GetPagedAsync(It.IsAny<PageRequest>(), 3, 8)).ReturnsAsync(PaginatedResult<StockMovementViewModel>.Create(new List<StockMovementViewModel>(), 0, new PageRequest()));
        var lookup = new Mock<ILookupService>(MockBehavior.Loose);
        lookup.Setup(a => a.NamedLookupAsync<Store>()).ReturnsAsync(new List<LookupItem> { new() { Id = 3 } });
        lookup.Setup(a => a.NamedLookupAsync<Product>()).ReturnsAsync(new List<LookupItem> { new() { Id = 8 } });
        var controller = new StockMovementController(service.Object, lookup.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };

        var model = Assert.IsType<StockMovementIndexViewModel>(Assert.IsType<ViewResult>(await controller.Index(null, 3, 8)).Model);

        Assert.Equal(3, model.StoreId);
        Assert.Equal(8, model.ProductId);
        Assert.Single(model.Stores);
        Assert.Single(model.Products);

        service.Setup(a => a.GetPagedAsync(It.IsAny<PageRequest>(), It.IsAny<int?>(), It.IsAny<int?>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await controller.Index(new PageRequest(), null, null)).ViewName);
    }

    // ---------------------------------------------------------------- duplicate checks used by the validators

    [Fact]
    public async Task ValidatorService_FindsDuplicatesOfAnotherRecord_NeverOfTheRecordItself_AndBlanksAreNeverDuplicates()
    {
        var job = new Job { Code = "1", ArName = "J", EnName = "J" };
        _host.Context.Add(job);
        await _host.Context.SaveChangesAsync();
        var employee = new Employee { Code = "E1", ArName = "علي حسن", EnName = "Ali Hassan", PhoneNumber = "01012345678", Job = job };
        _host.Context.Add(employee);
        await _host.Context.SaveChangesAsync();
        var validator = _host.Get<IValidatorService<Employee>>();

        Assert.True(validator.IsPhoneNumberExist(0, "01012345678"));
        Assert.False(validator.IsPhoneNumberExist(employee.Id, "01012345678"));
        Assert.False(validator.IsPhoneNumberExist(0, " "));

        Assert.True(validator.IsValueExist(0, "Code", "E1"));
        Assert.False(validator.IsValueExist(employee.Id, "Code", "E1"));
        Assert.False(validator.IsValueExist(0, "Code", ""));
        Assert.True(validator.IsArNameExist(0, "علي حسن"));
        Assert.True(validator.IsEnNameExist(0, "Ali Hassan"));
    }

    // ---------------------------------------------------------------- a cashier order started from a reservation

    [Fact]
    public async Task NewOrderFromAReservation_IsAHallOrderOnItsTable_WithTheGuestNameAndPhone()
    {
        var table = new Restaurant.DAL.Models.Table { Code = "1", ArName = "T1", EnName = "T1", NumOfPersons = 4 };
        _host.Context.Add(table);
        await _host.Context.SaveChangesAsync();
        var reservation = new TableReservation
        {
            TableId = table.Id, CustomerName = "Mona", PhoneNumber = "0100000000", GuestCount = 2,
            ReservationDate = DateTime.Today.AddDays(1), Status = ReservationStatus.Confirmed
        };
        _host.Context.Add(reservation);
        await _host.Context.SaveChangesAsync();
        _host.Context.ChangeTracker.Clear();
        var orders = _host.Get<IOrderService>();

        var withReservation = await orders.GetNewOrderAsync(table.Id, reservation.Id);
        var unknownReservation = await orders.GetNewOrderAsync(table.Id, 999);
        var noReservation = await orders.GetNewOrderAsync(table.Id, null);

        Assert.Equal(OrderType.Hall, withReservation.Type);
        Assert.Equal(table.Id, withReservation.TableId);
        Assert.Equal("Mona", withReservation.CustomerName);
        Assert.Equal("0100000000", withReservation.PhoneNumber);
        Assert.Null(unknownReservation.CustomerName);
        Assert.Null(noReservation.CustomerName);
        Assert.Equal(table.Id, noReservation.TableId);
    }

    // ---------------------------------------------------------------- phone rules (supplier form)

    private async Task<Dictionary<string, List<string>>> SupplierErrors(SupplierViewModel model)
    {
        var validator = (IValidator<SupplierViewModel>)_host.Get(typeof(IValidator<SupplierViewModel>));
        var result = await validator.ValidateAsync(model);

        return result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
    }

    [Fact]
    public async Task ExtraPhones_MustBeDigitsAndShort_AndNeverRepeatEachOtherOrTheMainNumber()
    {
        var fine = await SupplierErrors(new SupplierViewModel { ArName = "مورد", EnName = "Supplier", PhoneNumber = "01011111111", Phones = new List<PhoneViewModel> { new() { PhoneNumber = "0222222222" }, new() { PhoneNumber = "+20 33 4444" } } });
        var repeated = await SupplierErrors(new SupplierViewModel { ArName = "مورد", EnName = "Supplier", PhoneNumber = "01011111111", Phones = new List<PhoneViewModel> { new() { PhoneNumber = "0222222222" }, new() { PhoneNumber = " 0222222222 " } } });
        var sameAsMain = await SupplierErrors(new SupplierViewModel { ArName = "مورد", EnName = "Supplier", PhoneNumber = "01011111111", Phones = new List<PhoneViewModel> { new() { PhoneNumber = "01011111111" } } });
        var letters = await SupplierErrors(new SupplierViewModel { ArName = "مورد", EnName = "Supplier", PhoneNumber = "01011111111", Phones = new List<PhoneViewModel> { new() { PhoneNumber = "call-me" } } });
        var tooLong = await SupplierErrors(new SupplierViewModel { ArName = "مورد", EnName = "Supplier", PhoneNumber = "01011111111", Phones = new List<PhoneViewModel> { new() { PhoneNumber = new string('1', 21) } } });

        Assert.DoesNotContain(fine, e => e.Key.Contains("Phones"));
        Assert.Contains("PhoneAlreadyExists", repeated["Phones"]);
        Assert.Contains("PhoneAlreadyExists", sameAsMain["Phones"]);
        Assert.Contains(letters, e => e.Key.Contains("PhoneNumber") && e.Value.Contains("Val.PhoneDigitsOnly"));
        Assert.Contains(tooLong, e => e.Key.Contains("PhoneNumber") && e.Value.Any(m => m == "Val.MaxLength" || m.Contains("20")));
    }

    [Fact]
    public async Task CustomerAndStorePhones_FixedErpColumns_DigitsOnly_AndNeverRepeated()
    {
        var customerValidator = (IValidator<CustomerViewModel>)_host.Get(typeof(IValidator<CustomerViewModel>));
        var storeValidator = (IValidator<StoreViewModel>)_host.Get(typeof(IValidator<StoreViewModel>));

        var fine = await customerValidator.ValidateAsync(new CustomerViewModel { ArName = "عميل", EnName = "Customer", PhoneNumber = "0101", PhoneNumber2 = "0102" });
        var repeated = await customerValidator.ValidateAsync(new CustomerViewModel { ArName = "عميل", EnName = "Customer", PhoneNumber = "0101", PhoneNumber2 = " 0101 " });
        var letters = await storeValidator.ValidateAsync(new StoreViewModel { ArName = "مخزن", EnName = "Store", PhoneNumber = "0101", PhoneNumber2 = "call-me" });
        var storeRepeated = await storeValidator.ValidateAsync(new StoreViewModel { ArName = "مخزن", EnName = "Store", PhoneNumber = "0101", PhoneNumber2 = "0102", PhoneNumber3 = "0102" });

        Assert.DoesNotContain(fine.Errors, e => e.PropertyName.Contains("Phone"));
        Assert.Contains(repeated.Errors, e => e.ErrorMessage == "PhoneAlreadyExists");
        Assert.Contains(letters.Errors, e => e.PropertyName == "PhoneNumber2" && e.ErrorMessage == "Val.PhoneDigitsOnly");
        Assert.Contains(storeRepeated.Errors, e => e.ErrorMessage == "PhoneAlreadyExists");
    }

    // ---------------------------------------------------------------- asset rules of the account form

    private async Task<Dictionary<string, List<string>>> AccountErrors(AccountDefinitionViewModel model)
    {
        var validator = (IValidator<AccountDefinitionViewModel>)_host.Get(typeof(IValidator<AccountDefinitionViewModel>));
        var result = await validator.ValidateAsync(model);

        return result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
    }

    private static AccountDefinitionViewModel Asset(Action<AccountDefinitionViewModel> change)
    {
        var model = new AccountDefinitionViewModel { Kind = AccountKind.Asset, ArName = "أصل", EnName = "Asset", Code = "A1", BuyValue = 1000, ProductAge = 5, BuyDate = new DateTime(2026, 1, 1), StartUsingDate = new DateTime(2026, 2, 1) };
        change(model);

        return model;
    }

    [Fact]
    public async Task Asset_ValueAgeAndDatesFollowTheirRules()
    {
        var fine = await AccountErrors(Asset(_ => { }));
        var negativeBuy = await AccountErrors(Asset(m => m.BuyValue = -1));
        var scrapAboveBuy = await AccountErrors(Asset(m => m.ScrapValue = 2000));
        var negativeScrap = await AccountErrors(Asset(m => m.ScrapValue = -5));
        var noAge = await AccountErrors(Asset(m => m.ProductAge = null));
        var usedBeforeBought = await AccountErrors(Asset(m => m.StartUsingDate = new DateTime(2025, 12, 1)));
        var longLocation = await AccountErrors(Asset(m => m.Location = new string('x', 201)));

        Assert.DoesNotContain(fine, e => e.Key is "BuyValue" or "ScrapValue" or "ProductAge" or "StartUsingDate" or "Location");
        Assert.Contains("Val.RequiredField", negativeBuy["BuyValue"]);
        Assert.Contains("Asset.ScrapAboveBuy", scrapAboveBuy["ScrapValue"]);
        Assert.Contains("Val.RequiredField", negativeScrap["ScrapValue"]);
        Assert.Contains("Asset.AgeRequired", noAge["ProductAge"]);
        Assert.Contains("Asset.UseBeforeBuy", usedBeforeBought["StartUsingDate"]);
        Assert.True(longLocation.ContainsKey("Location"));
    }
}
