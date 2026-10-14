using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.Definitions;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.BLL.Validations.Branch;
using Restaurant.BLL.Services.Commons;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Repositories.Definitions;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.UnitOfWork;
using ResturantPL.Extensions;
using ResturantPL.Helper;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.Options;
using System.Globalization;

namespace ResturantPL;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddWebServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        services.AddScoped<IToastrService, ToastrService>();

        services.AddMemoryCache();
        services.Configure<InMemoryCacheOptions>(options =>
        {
            options.SlidingExpiration = "00:30:00";
            options.AbsoluteExpirationRelativeToNow = "00:00:00";
        });
        services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var arCulture = new CultureInfo("ar");
            arCulture.DateTimeFormat.Calendar = new GregorianCalendar();
            arCulture.NumberFormat.NativeDigits = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            arCulture.NumberFormat.NumberDecimalSeparator = ".";
            arCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            arCulture.NumberFormat.PercentDecimalSeparator = ".";
            arCulture.NumberFormat.NumberGroupSeparator = ",";
            arCulture.NumberFormat.CurrencyGroupSeparator = ",";
            arCulture.NumberFormat.PercentGroupSeparator = ",";
            arCulture.DateTimeFormat.DateSeparator = "/";
            arCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";

            var enCulture = new CultureInfo("en");
            var supportedCultures = new[] { arCulture, enCulture };

            options.DefaultRequestCulture = new RequestCulture(arCulture, arCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            };
        });

        services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
        services.AddControllersWithViews(options =>
            {
                options.Filters.Add<ResturantPL.Filters.AjaxRedirectFilter>();
                if (builder.Configuration.GetValue<bool>("Security:Relaxed"))
                {
                    options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute
                    {
                        Order = 1001
                    });
                }
                else
                {
                    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
                }
            }).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();
        services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new DefinitionsViewLocationExpander()));
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<BranchVmValidator>();

        services.AddAutoMapper(cfg => { }, typeof(Restaurant.BLL.Helper.AutoMapper).Assembly);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IValidatorRepository<>), typeof(ValidatorRepository<>));
        services.AddScoped(typeof(IValidatorService<>), typeof(ValidatorService<>));
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IHomeService, HomeService>();
        services.AddScoped<IChefService, ChefService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IUnitPriceService, UnitPriceService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IMenuSectionService, MenuSectionService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<Restaurant.DAL.IRepositories.Definitions.IRoleRepository,
            Restaurant.DAL.Repositories.Definitions.RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOnlineOrderService, OnlineOrderService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IStoreConvertService, StoreConvertService>();
        services.AddScoped<ISupplierPaymentService, SupplierPaymentService>();
        services.AddScoped<ICustomerPaymentService, CustomerPaymentService>();
        services.AddScoped<Restaurant.DAL.IRepositories.Definitions.IUserRepository,
            Restaurant.DAL.Repositories.Definitions.UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<Restaurant.DAL.IRepositories.Definitions.IEmployeeAccountRepository,
            Restaurant.DAL.Repositories.Definitions.EmployeeAccountRepository>();
        services.AddScoped<IEmployeeAccountService, EmployeeAccountService>();
        services.AddScoped<Restaurant.DAL.IRepositories.Definitions.ISettingRepository,
            Restaurant.DAL.Repositories.Definitions.SettingRepository>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<Restaurant.DAL.IRepositories.Definitions.IAccountRepository,
            Restaurant.DAL.Repositories.Definitions.AccountRepository>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<IExitExpenseService, ExitExpenseService>();
        services.AddScoped<IScroungeService, ScroungeService>();
        services.AddScoped<ISalaryService, SalaryService>();
        services.AddScoped(typeof(ILookupCrudRepository<>), typeof(LookupCrudRepository<>));
        services.AddScoped(typeof(ILookupCrudService<>), typeof(LookupCrudService<>));
        services.AddScoped<IKitchenService, KitchenService>();
        services.AddScoped<IDeliveryZoneService, DeliveryZoneService>();
        services.AddScoped<IExpenseService, ExpenseService>();

        return builder;
    }
}
