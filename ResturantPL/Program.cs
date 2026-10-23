using Microsoft.AspNetCore.Mvc.Razor;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Restaurant.BLL.Services.Commons;
using Restaurant.BLL.Services.Definitions;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.BLL.Validations.Branch;
using Restaurant.DAL;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Restaurant.DAL.SeedData;
using Restaurant.DAL.UnitOfWork;
using ResturantPL.Filters;
using ResturantPL.Services;
using Rotativa.AspNetCore;
using Shared.Helpers;
using Shared.Helpers.Localization;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Default")));
services.AddHttpContextAccessor();
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

services.AddMemoryCache();
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

    var supportedCultures = new[] { arCulture, new CultureInfo("en") };
    options.DefaultRequestCulture = new RequestCulture(arCulture, arCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});

var relaxedSecurity = configuration.GetValue<bool>("Security:Relaxed");
services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
services.AddControllersWithViews(options =>
    {
        options.Filters.Add<AjaxRedirectFilter>();
        if (relaxedSecurity)
        {
            options.Filters.Add(new IgnoreAntiforgeryTokenAttribute { Order = 1001 });
        }
        else
        {
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        }
    })
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
services.AddValidatorsFromAssemblyContaining<BranchVmValidator>();
services.AddAutoMapper(cfg => { }, typeof(Restaurant.BLL.Helper.AutoMapper).Assembly);

services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.User.RequireUniqueEmail = true;
        // empty = any letters, so the Arabic user name المدير is allowed
        options.User.AllowedUserNameCharacters = "";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.AccessDeniedPath = "/Home/Error";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(6);

        options.Events.OnValidatePrincipal = async context =>
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            if (userId == null || await users.FindByIdAsync(userId) == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

if (relaxedSecurity)
{
    services.AddAuthorizationBuilder()
        .AddPolicy("Staff", policy => policy.RequireAuthenticatedUser())
        .AddPolicy("Management", policy => policy.RequireAuthenticatedUser())
        .AddPolicy("Admin", policy => policy.RequireAuthenticatedUser());
}
else
{
    services.AddAuthorizationBuilder()
        .AddPolicy("Staff", policy => policy.RequireAuthenticatedUser())
        .AddPolicy("Management", policy => policy.RequireRole("Admin", "Manager"))
        .AddPolicy("Admin", policy => policy.RequireRole("Admin"));
}

services.Configure<EmailOptions>(configuration.GetSection("Email"));
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IToastrService, ToastrService>();

services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped(typeof(IValidatorRepository<>), typeof(ValidatorRepository<>));
services.AddScoped(typeof(IValidatorService<>), typeof(ValidatorService<>));
services.AddScoped<ILookupService, LookupService>();
services.AddScoped<IBranchService, BranchService>();
services.AddScoped<IJobService, JobService>();
services.AddScoped<ITableService, TableService>();
services.AddScoped<ITableReservationService, TableReservationService>();
services.AddScoped<IUnitPriceService, UnitPriceService>();
services.AddScoped<IProductCategoryService, ProductCategoryService>();
services.AddScoped<IMenuSectionService, MenuSectionService>();
services.AddScoped<IMenuItemService, MenuItemService>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IStoreService, StoreService>();
services.AddScoped<ISupplierService, SupplierService>();
services.AddScoped<IRoleRepository, RoleRepository>();
services.AddScoped<IRoleService, RoleService>();
services.AddScoped<IInvoiceService, InvoiceService>();
services.AddScoped<IStockMovementService, StockMovementService>();
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IBillService, BillService>();
services.AddScoped<IAccountDefinitionService, AccountDefinitionService>();
services.AddScoped<IEntryService, EntryService>();
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IStoreConvertService, StoreConvertService>();
services.AddScoped<ISupplierPaymentService, SupplierPaymentService>();
services.AddScoped<ICustomerPaymentService, CustomerPaymentService>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IEmployeeAccountRepository, EmployeeAccountRepository>();
services.AddScoped<IEmployeeAccountService, EmployeeAccountService>();
services.AddScoped<ISettingRepository, SettingRepository>();
services.AddScoped<ISettingService, SettingService>();
services.AddScoped<IAccountSettingRepository, AccountSettingRepository>();
services.AddScoped<IAccountSettingService, AccountSettingService>();
services.AddScoped<IAccountRepository, AccountRepository>();
services.AddScoped<IAccountService, AccountService>();
services.AddScoped<ICustomerService, CustomerService>();
services.AddScoped<IEmployeeService, EmployeeService>();
services.AddScoped<IDashboardService, DashboardService>();
services.AddScoped<IDepositService, DepositService>();
services.AddScoped<IExitExpenseService, ExitExpenseService>();
services.AddScoped<IScroungeService, ScroungeService>();
services.AddScoped<ISalaryService, SalaryService>();
services.AddScoped<IKitchenService, KitchenService>();
services.AddScoped<ICurrencyService, CurrencyService>();
services.AddScoped<ICompanyService, CompanyService>();
services.AddScoped<ILeaveTypeService, LeaveTypeService>();
services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();
services.AddScoped<IAllowanceTypeService, AllowanceTypeService>();
services.AddScoped<IDeductionTypeService, DeductionTypeService>();
services.AddScoped<IInsuranceCategoryService, InsuranceCategoryService>();
services.AddScoped<IJobTypeService, JobTypeService>();
services.AddScoped<IDepartmentService, DepartmentService>();
services.AddScoped<IContractTypeService, ContractTypeService>();
services.AddScoped<ISpecializationService, SpecializationService>();
services.AddScoped<ICustomerTierService, CustomerTierService>();
services.AddScoped<IClassificationService, ClassificationService>();
services.AddScoped<ICostCenterService, CostCenterService>();
services.AddScoped<ISellTypeService, SellTypeService>();
services.AddScoped<IDeliveryZoneService, DeliveryZoneService>();
services.AddScoped<IExpenseService, ExpenseService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

ErrorLog.Logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("App");
ErrorLog.ShowDetails = app.Environment.IsDevelopment();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseSession();
app.UseRotativa();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var defaultPassword = configuration["Database:DefaultPassword"] ?? (app.Environment.IsDevelopment() ? "@Qwe12" : null);

    await DataSeeder.SeedCountries(dbContext);
    await DataSeeder.SeedGovs(dbContext);
    await DataSeeder.SeedTowns(dbContext);
    await DataSeeder.SeedApplicationSettings(dbContext, defaultPassword);
    await DataSeeder.SeedPrinterSettings(dbContext);
    await DataSeeder.SeedAdminRoleAndUsers(userManager, roleManager, defaultPassword != null, defaultPassword);
}

app.Run();
