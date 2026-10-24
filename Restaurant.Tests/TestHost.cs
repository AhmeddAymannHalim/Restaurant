using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Restaurant.BLL.Services.Commons;
using Restaurant.DAL;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.Helpers.Localization;

namespace Restaurant.Tests;

// The app's real dependency graph (like Program.cs) on the SQLite test database:
// every service, repository, validator, controller, the mapper and Identity. Only the toastr, e-mail and localizer are fakes.
public class TestHost : IDisposable
{
    private readonly TestDb _db;
    private readonly ServiceProvider _provider;

    public IServiceProvider Services => _provider;
    public ApplicationDbContext Context => _db.Context;
    public TestDb Db => _db;
    public Mock<IToastrService> Toastr { get; } = new();
    public Mock<ResturantPL.Services.IEmailService> Email { get; } = new();

    public TestHost(bool invoiceAccounts = false)
    {
        _db = new TestDb(invoiceAccounts);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(_db.Context);
        services.AddSingleton(_db.Http);
        services.AddSingleton(Toastr.Object);
        services.AddSingleton(Email.Object);
        services.AddSingleton(typeof(IStringLocalizer<>), typeof(KeyLocalizer<>));
        services.AddSingleton(new MapperConfiguration(cfg => cfg.AddProfile<Restaurant.BLL.Helper.AutoMapper>(), NullLoggerFactory.Instance).CreateMapper());

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication().AddCookie(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IValidatorRepository<>), typeof(Restaurant.DAL.Repositories.Commons.ValidatorRepository<>));
        services.AddScoped(typeof(IValidatorService<>), typeof(ValidatorService<>));

        // every XService : IXService, and every XRepository : IXRepository (the rest are generic and registered above)
        Register(services, typeof(Restaurant.BLL.Services.Definitions.KitchenService).Assembly, "Service");
        Register(services, typeof(Restaurant.DAL.Repositories.Definitions.KitchenRepository).Assembly, "Repository");
        services.AddScoped<ILookupService, LookupService>();
        services.AddValidatorsFromAssemblyContaining<Restaurant.BLL.Validations.Branch.BranchVmValidator>();

        foreach (var controller in typeof(ResturantPL.Controllers.Definitions.KitchenController).Assembly.GetTypes()
                     .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract))
        {
            services.AddTransient(controller);
        }

        _provider = services.BuildServiceProvider();
    }

    private static void Register(IServiceCollection services, Assembly assembly, string suffix)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && t.Name.EndsWith(suffix)))
        {
            var contract = type.GetInterfaces().FirstOrDefault(i => i.Name == "I" + type.Name);
            if (contract != null)
            {
                services.AddScoped(contract, type);
            }
        }
    }

    public T Get<T>() => _provider.GetRequiredService<T>();

    public object Get(Type type) => _provider.GetRequiredService(type);

    // a signed-in manager, like the real pipeline would give the controller
    public static ClaimsPrincipal Manager() => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "user-1"),
        new Claim(ClaimTypes.Name, "Halim"),
        new Claim(ClaimTypes.Email, "halim@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    }, "test"));

    // a controller wired like the real pipeline would: signed-in manager, session, url helper and temp data
    public T Controller<T>() where T : Microsoft.AspNetCore.Mvc.Controller => Configure((T)Get(typeof(T)));

    public T Configure<T>(T controller) where T : Microsoft.AspNetCore.Mvc.Controller
    {
        var http = new DefaultHttpContext { RequestServices = _provider, User = Manager() };
        http.Request.Scheme = "http";
        http.Request.Host = new HostString("127.0.0.1");
        http.Features.Set<Microsoft.AspNetCore.Http.Features.ISessionFeature>(new SessionFeature { Session = new FakeSession() });
        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Action(It.IsAny<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>())).Returns("/x");
        url.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        controller.ControllerContext = new ControllerContext(new Microsoft.AspNetCore.Mvc.ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()));
        controller.Url = url.Object;
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(http, Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

        return controller;
    }

    public void Dispose()
    {
        _provider.Dispose();
        _db.Dispose();
    }
}

// the localizer returns the key, so tests can assert on message keys
public class KeyLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
}
