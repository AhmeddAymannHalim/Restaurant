using FluentValidation;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

public class ValidatorTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private static IEnumerable<Type> ModelTypes() => typeof(Restaurant.BLL.Validations.Branch.BranchVmValidator).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
        .Select(t => t.BaseType.GetGenericArguments()[0])
        .Distinct();

    public static IEnumerable<object[]> Models() => ModelTypes().Select(t => new object[] { t.Name });

    private IValidator For(Type model) => (IValidator)_host.Get(typeof(IValidator<>).MakeGenericType(model));

    private async Task<Dictionary<string, List<string>>> Errors(object model)
    {
        var result = await For(model.GetType()).ValidateAsync(new ValidationContext<object>(model));

        return result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
    }

    // ---------------------------------------------------------------- every validator

    [Theory]
    [MemberData(nameof(Models))]
    public async Task EveryValidator_HandlesAnEmptyAndAFilledForm_WithoutCrashing(string modelName)
    {
        var type = ModelTypes().Single(t => t.Name == modelName);
        var validator = For(type);

        await validator.ValidateAsync(new ValidationContext<object>(Activator.CreateInstance(type)));
        await validator.ValidateAsync(new ValidationContext<object>(Args.Make(type)));

        Assert.NotNull(validator);
    }

    // ---------------------------------------------------------------- the 25 definition forms: required and duplicate names

    public static IEnumerable<object[]> Definitions() => DefinitionRepositoryTests.Repositories()
        .Select(a => new object[] { ((Type)a[0]).Name.Replace("Repository", string.Empty) });

    [Theory]
    [MemberData(nameof(Definitions))]
    public async Task DefinitionForm_EmptyNames_AreRequired_AndRepeatedNamesAreRefused(string name)
    {
        var model = ModelTypes().SingleOrDefault(t => t.Name == name + "ViewModel");
        if (model is null)
        {
            return;
        }

        var empty = Activator.CreateInstance(model);
        var emptyErrors = await Errors(empty);
        Assert.Contains("ArName", emptyErrors.Keys);
        Assert.Contains("EnName", emptyErrors.Keys);
        Assert.All(emptyErrors["ArName"], m => Assert.Contains("Val.", m));

        // save one through the repository, then post the same names as a new record
        var repoType = DefinitionRepositoryTests.Repositories().Select(a => (Type)a[0]).Single(t => t.Name == name + "Repository");
        var entityType = DefinitionRepositoryTests.Repositories().Where(a => (Type)a[0] == repoType).Select(a => (Type)a[1]).Single();
        var entity = Activator.CreateInstance(entityType);
        entityType.GetProperty("ArName").SetValue(entity, "Same Name");
        entityType.GetProperty("EnName").SetValue(entity, "Same Name");
        entityType.GetProperty("Code")?.SetValue(entity, "900");
        _host.Context.Add(entity);
        await _host.Context.SaveChangesAsync();
        var savedId = (int)entityType.GetProperty("Id").GetValue(entity);

        var again = Activator.CreateInstance(model);
        model.GetProperty("ArName").SetValue(again, "Same Name");
        model.GetProperty("EnName").SetValue(again, "Same Name");
        var duplicate = await Errors(again);
        Assert.Contains("Val.NameAlreadyExist", duplicate.GetValueOrDefault("ArName") ?? new List<string>());

        // editing that same record keeps its own name without a complaint
        model.GetProperty("Id").SetValue(again, savedId);
        var self = await Errors(again);
        Assert.DoesNotContain("Val.NameAlreadyExist", self.GetValueOrDefault("ArName") ?? new List<string>());
    }

    // ---------------------------------------------------------------- the money forms

    [Fact]
    public async Task Payment_NeedsPartnerInvoiceAndAPositiveAmount()
    {
        var errors = await Errors(new PaymentViewModel { Paid = 0 });

        Assert.Contains("PartnerId", errors.Keys);
        Assert.Contains("BillMasterId", errors.Keys);
        Assert.Contains("Val.GreaterThanZero", errors["Paid"]);
        Assert.Empty(await Errors(new PaymentViewModel { PartnerId = 1, BillMasterId = 1, Paid = 10 }));
        Assert.Contains("Paid", (await Errors(new PaymentViewModel { PartnerId = 1, BillMasterId = 1, Paid = -5 })).Keys);
    }

    [Fact]
    public async Task User_NeedsAPhoneNumber()
    {
        var user = new UserViewModel { UserName = "sara", FirstName = "Sara", LastName = "A", Email = "sara@test.com", UserType = "Waiter", Password = "Pass123", ConfirmPassword = "Pass123" };

        Assert.Contains("PhoneNumber", (await Errors(user)).Keys);

        user.PhoneNumber = "01011112222";
        Assert.Empty(await Errors(user));
    }

    [Fact]
    public async Task Order_Rules()
    {
        var item = new OrderItemViewModel { MenuItemId = 1, Quantity = 1 };

        Assert.Contains("Items", (await Errors(new OrderViewModel())).Keys);
        Assert.Contains("Val.GreaterThanZero", (await Errors(new OrderViewModel { Items = { new OrderItemViewModel { MenuItemId = 1, Quantity = 0 } } })).SelectMany(e => e.Value));
        Assert.Contains("CustomerId", (await Errors(new OrderViewModel { Items = { item }, OnAccount = true })).Keys);
        Assert.Contains("TableId", (await Errors(new OrderViewModel { Items = { item }, Type = OrderType.Hall, Cash = false })).Keys);
        Assert.Contains("EmployeeId", (await Errors(new OrderViewModel { Items = { item }, Type = OrderType.Delivery, Cash = false, Address = "street" })).Keys);
        Assert.Contains("Address", (await Errors(new OrderViewModel { Items = { item }, Type = OrderType.Delivery, Cash = true, EmployeeId = 1 })).Keys);
        Assert.Contains("Discount", (await Errors(new OrderViewModel { Items = { item }, Type = OrderType.TakeAway, Cash = true, Discount = -1 })).Keys);
        Assert.Contains("PhoneNumber", (await Errors(new OrderViewModel { Items = { item }, Type = OrderType.TakeAway, Cash = true, PhoneNumber = "123456789012" })).Keys);
        Assert.Empty(await Errors(new OrderViewModel { Items = { item }, Type = OrderType.TakeAway, Cash = true }));
    }

    [Fact]
    public async Task Invoice_Rules()
    {
        var line = new InvoiceItemViewModel { ProductId = 1, UnitPriceId = 1, Quantity = 2, Price = 10 };
        InvoiceViewModel Good() => new() { InvoiceType = InvoiceType.Purchases, SupplierId = 1, StoreId = 1, Items = { line } };

        Assert.Empty(await Errors(Good()));
        Assert.Contains("Items", (await Errors(new InvoiceViewModel { InvoiceType = InvoiceType.Purchases, SupplierId = 1, StoreId = 1 })).Keys);

        var noSupplier = Good();
        noSupplier.SupplierId = null;
        Assert.Contains("SupplierId", (await Errors(noSupplier)).Keys);

        var noCustomer = Good();
        noCustomer.InvoiceType = InvoiceType.Sales;
        noCustomer.SupplierId = null;
        Assert.Contains("CustomerId", (await Errors(noCustomer)).Keys);

        var noStore = Good();
        noStore.StoreId = null;
        Assert.Contains("StoreId", (await Errors(noStore)).Keys);

        var overpaid = Good();
        overpaid.Paid = 21;
        Assert.Contains("Val.PaidTooBig", (await Errors(overpaid))["Paid"]);

        var bigDiscount = Good();
        bigDiscount.Discount = 21;
        Assert.Contains("Val.DiscountTooBig", (await Errors(bigDiscount))["Discount"]);

        var negative = Good();
        negative.Discount = -1;
        negative.Paid = -1;
        var errors = await Errors(negative);
        Assert.Contains("Val.NotNegative", errors["Discount"]);
        Assert.Contains("Val.NotNegative", errors["Paid"]);

        var badLine = Good();
        badLine.Items[0] = new InvoiceItemViewModel { ProductId = 0, UnitPriceId = 0, Quantity = 0, Price = -1 };
        Assert.True((await Errors(badLine)).Count >= 3);
    }

    [Fact]
    public async Task InvoiceWithLinkedPayments_PaidPlusPaymentsCannotPassTheTotal()
    {
        var invoice = new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Purchases, SupplierId = 1, StoreId = 1, Paid = 10, LinkedPayments = 15,
            Items = { new InvoiceItemViewModel { ProductId = 1, UnitPriceId = 1, Quantity = 2, Price = 10 } }
        };

        Assert.Contains("Val.PaidTooBig", (await Errors(invoice))["Paid"]);
        invoice.Paid = 5;
        Assert.Empty(await Errors(invoice));
    }

    // ---------------------------------------------------------------- sign in, passwords, accounts

    [Fact]
    public async Task EmployeeAccount_Password_MinimumSix_AndConfirmationMustMatch()
    {
        var weak = await Errors(new EmployeeAccountViewModel { EmployeeId = 1, UserName = "omar", Email = "o@test.com", Password = "12345", ConfirmPassword = "12345" });
        Assert.Contains("Password", weak.Keys);

        var mismatch = await Errors(new EmployeeAccountViewModel { EmployeeId = 1, UserName = "omar", Email = "o@test.com", Password = "Pass123", ConfirmPassword = "Other123" });
        Assert.Contains("ConfirmPassword", mismatch.Keys);

        var good = await Errors(new EmployeeAccountViewModel { EmployeeId = 1, UserName = "omar", Email = "o@test.com", Password = "Pass123", ConfirmPassword = "Pass123" });
        Assert.DoesNotContain("Password", good.Keys);
        Assert.DoesNotContain("ConfirmPassword", good.Keys);
    }

    [Fact]
    public async Task SignIn_NeedsEmailAndPassword()
    {
        var errors = await Errors(new SignInViewModel());

        Assert.Contains("Email", errors.Keys);
        Assert.Contains("Password", errors.Keys);
        Assert.Empty(await Errors(new SignInViewModel { Email = "a@test.com", Password = "x" }));
    }

    // ---------------------------------------------------------------- partners and people

    [Fact]
    public async Task Employee_PhoneMustBeElevenDigits_AndRequiredFieldsAreChecked()
    {
        var empty = await Errors(new EmployeeViewModel());
        foreach (var field in new[] { "ArName", "EnName", "PhoneNumber", "TownId", "Address", "JobId", "BranchId" })
        {
            Assert.Contains(field, empty.Keys);
        }

        var shortPhone = await Errors(new EmployeeViewModel { PhoneNumber = "123" });
        Assert.Contains("PhoneNumber", shortPhone.Keys);
    }

    [Fact]
    public async Task Customer_And_Supplier_RepeatedNameOfAnotherAccountIsRefused_ButTheirOwnAccountIsFine()
    {
        var account = new Account { Code = "C1", ArName = "Ali", EnName = "Ali", AccountType = AccountType.Customer };
        _host.Context.Customers.Add(new Customer { Account = account });
        await _host.Context.SaveChangesAsync();

        var another = await Errors(new CustomerViewModel { ArName = "Ali", EnName = "Ali" });
        Assert.Contains("Val.NameAlreadyExist", another["ArName"]);

        var itself = await Errors(new CustomerViewModel { AccountId = account.Id, ArName = "Ali", EnName = "Ali" });
        Assert.DoesNotContain("Val.NameAlreadyExist", itself.GetValueOrDefault("ArName") ?? new List<string>());
    }
}
