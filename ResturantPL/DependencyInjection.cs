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

        // Toastr
        services.AddScoped<IToastrService, ToastrService>();

        // Cache
        services.AddMemoryCache();
        services.Configure<InMemoryCacheOptions>(options =>
        {
            options.SlidingExpiration = "00:30:00";
            options.AbsoluteExpirationRelativeToNow = "00:00:00";
        });
        services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

        // Localization (json files in /Resources)
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
            // The restaurant starts in Arabic; the browser language header is ignored, only the language button (cookie) changes it.
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            };
        });

        // MVC + view localization + FluentValidation
        services.AddControllersWithViews().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new DefinitionsViewLocationExpander()));
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<BranchVmValidator>();

        // AutoMapper
        services.AddAutoMapper(cfg => { }, typeof(Profiler).Assembly, typeof(Restaurant.BLL.Helper.AutoMapper).Assembly);

        // DAL / BLL
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IValidatorRepository<>), typeof(ValidatorRepository<>));
        services.AddScoped(typeof(IValidatorService<>), typeof(ValidatorService<>));
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<IChefService, ChefService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IUnitPriceService, UnitPriceService>();
        services.AddScoped<ISalaryTypeService, SalaryTypeService>();
        services.AddScoped<IMenuSectionService, MenuSectionService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<IExitExpenseService, ExitExpenseService>();
        services.AddScoped<IScroungeService, ScroungeService>();
        services.AddScoped<ISalaryService, SalaryService>();
        services.AddScoped<IKitchenService, KitchenService>();
        services.AddScoped<IExpenseService, ExpenseService>();

        return builder;
    }
}
