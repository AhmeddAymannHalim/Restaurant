using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Lookups, the duplicate checks used by the validators, the stock movement list, the dashboard and the location helper.
public class ReadSideRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    // ---------------------------------------------------------------- validators

    [Fact]
    public async Task Validator_CodeAndNames_IgnoreSpacesAndCase_AndSkipTheRecordItself()
    {
        var kitchens = new KitchenRepository(_db.Context, _db.Http);
        var kitchen = new Kitchen { ArName = "مطبخ رئيسي", EnName = "Main Kitchen" };
        await kitchens.UpsertKitchenAsync(kitchen);
        var sut = new ValidatorRepository<Kitchen>(_db.Context);

        Assert.True(sut.IsCodeExist(0, " 1 "));
        Assert.False(sut.IsCodeExist(kitchen.Id, "1"));
        Assert.True(sut.IsEnNameExist(0, "mainkitchen"));
        Assert.True(sut.IsEnNameExist(0, "MAIN  KITCHEN"));
        Assert.False(sut.IsEnNameExist(kitchen.Id, "Main Kitchen"));
        Assert.True(sut.IsArNameExist(0, "مطبخرئيسي"));
        Assert.False(sut.IsArNameExist(0, "   "));
        Assert.False(sut.IsCodeExist(0, null));
        Assert.True(sut.IsValueExist(0, "EnName", "Main Kitchen"));
        Assert.False(sut.IsValueExist(0, "EnName", "main kitchen")); // exact match by design
        Assert.False(sut.IsValueExist(kitchen.Id, "EnName", "Main Kitchen"));
    }

    // ---------------------------------------------------------------- lookups

    [Fact]
    public async Task Lookups_ReturnTheRowsInIdOrder_WithBothNames()
    {
        var sut = new LookupRepository(_db.Context);
        var kitchens = new KitchenRepository(_db.Context, _db.Http);
        await kitchens.UpsertKitchenAsync(new Kitchen { ArName = "ب", EnName = "B" });
        await kitchens.UpsertKitchenAsync(new Kitchen { ArName = "أ", EnName = "A" });
        _db.Context.Stores.Add(new Store { Code = "1", ArName = "مخزن", EnName = "Store" });
        _db.Context.Tables.AddRange(new Table { Code = "1", ArName = "T1", EnName = "T1", IsPending = true }, new Table { Code = "2", ArName = "T2", EnName = "T2" });
        _db.Context.Expenses.Add(new Expense { Code = "1", ArName = "إيجار", EnName = "Rent" });
        _db.Context.Jobs.Add(new Job { Code = "1", ArName = "طيار", EnName = "Driver" });
        _db.Context.UnitPrices.Add(new UnitPrice { Code = "1", ArName = "كيلو", EnName = "Kg" });
        await _db.Context.SaveChangesAsync();

        var list = (await sut.NamedLookupAsync<Kitchen>()).ToList();
        Assert.Equal(new[] { "B", "A" }, list.Select(a => a.EnName));
        Assert.Single(await sut.NamedLookupAsync<Store>());
        Assert.Equal(2, (await sut.NamedLookupAsync<Restaurant.DAL.Models.Table>()).Count());
        Assert.Equal("T2", Assert.Single(await sut.FreeTableLookupAsync()).EnName);
        Assert.Single(await sut.NamedLookupAsync<Expense>());
        Assert.Single(await sut.NamedLookupAsync<Job>());
        Assert.Single(await sut.NamedLookupAsync<UnitPrice>());
        Assert.Empty(await sut.NamedLookupAsync<Product>());
        Assert.Empty(await sut.NamedLookupAsync<MenuSection>());
        Assert.Empty(await sut.NamedLookupAsync<MenuSection>());
        Assert.Empty(await sut.NamedLookupAsync<CompanyBranch>());
        Assert.Empty(await sut.NamedLookupAsync<ProductCategory>());
        Assert.Empty(await sut.NamedLookupAsync<Employee>());
    }

    [Fact]
    public async Task Lookups_DriversAndStorekeepersComeFromTheirJobs()
    {
        var sut = new LookupRepository(_db.Context);
        var driverJob = new Job { Code = "1", ArName = "طيار", EnName = "Driver" };
        var keeperJob = new Job { Code = "2", ArName = "أمين مخزن", EnName = "Store Keeper" };
        var cookJob = new Job { Code = "3", ArName = "طباخ", EnName = "Cook" };
        _db.Context.Employees.AddRange(
            new Employee { Code = "1", ArName = "سائق", EnName = "Driver1", Job = driverJob },
            new Employee { Code = "2", ArName = "أمين", EnName = "Keeper1", Job = keeperJob },
            new Employee { Code = "3", ArName = "طباخ", EnName = "Cook1", Job = cookJob });
        await _db.Context.SaveChangesAsync();

        Assert.Equal("Driver1", Assert.Single(await sut.DeliveryDriverLookupAsync()).EnName);
        Assert.Equal("Keeper1", Assert.Single(await sut.StorekeeperLookupAsync()).EnName);
        Assert.Equal(3, (await sut.NamedLookupAsync<Employee>()).Count());
    }

    [Fact]
    public async Task Lookups_CustomersAndSuppliersShowTheNameOfTheirAccount()
    {
        var sut = new LookupRepository(_db.Context);
        _db.Context.Customers.Add(new Customer { Account = new Account { Code = "C", ArName = "عميل", EnName = "Cus" } });
        _db.Context.Suppliers.Add(new Supplier { Account = new Account { Code = "S", ArName = "مورد", EnName = "Sup" } });
        await _db.Context.SaveChangesAsync();

        Assert.Equal("Cus", Assert.Single(await sut.CustomerLookupAsync()).EnName);
        Assert.Equal("Sup", Assert.Single(await sut.SupplierLookupAsync()).EnName);
    }

    [Fact]
    public async Task Lookups_LocationChain_CountryGovernorateTownVillage()
    {
        var sut = new LookupRepository(_db.Context);
        var country = new Country { ArName = "مصر", EnName = "Egypt" };
        var gov = new Governorate { ArName = "القاهرة", EnName = "Cairo", Country = country };
        var otherGov = new Governorate { ArName = "الجيزة", EnName = "Giza", Country = country };
        var town = new Town { ArName = "مدينة", EnName = "Town", Governorate = gov };
        _db.Context.Villages.Add(new Village { ArName = "قرية", EnName = "Village", Town = town });
        _db.Context.Governorates.Add(otherGov);
        await _db.Context.SaveChangesAsync();

        Assert.Single(await sut.NamedLookupAsync<Country>());
        Assert.Equal(2, (await sut.GovernorateLookupAsync(country.Id)).Count());
        Assert.Empty(await sut.GovernorateLookupAsync(999));
        Assert.Equal("Town", Assert.Single(await sut.CityLookupAsync(gov.Id)).EnName);
        Assert.Empty(await sut.CityLookupAsync(otherGov.Id));
        Assert.Single(await sut.VillageLookupAsync(town.Id));
    }

    [Fact]
    public async Task Lookups_StoreProducts_ShowTheQuantityOfThatStoreOnly()
    {
        var sut = new LookupRepository(_db.Context);
        var a = new Store { Code = "1", ArName = "A", EnName = "A" };
        var b = new Store { Code = "2", ArName = "B", EnName = "B" };
        var flour = new Product { Code = "1", ArName = "دقيق", EnName = "Flour", Cost = 4 };
        var sugar = new Product { Code = "2", ArName = "سكر", EnName = "Sugar", Cost = 6 };
        _db.Context.AddRange(a, b, flour, sugar);
        await _db.Context.SaveChangesAsync();
        var stock = new StockRepository(_db.Context);
        await stock.AdjustAsync(a.Id, flour.Id, 9);
        await stock.AdjustAsync(b.Id, flour.Id, 2);

        var items = (await sut.StoreProductLookupByStoreIdAsync(a.Id)).ToList();

        Assert.Equal(2, items.Count);
        Assert.Equal(9, items.Single(i => i.ProductId == flour.Id).Quantity);
        Assert.Equal(0, items.Single(i => i.ProductId == sugar.Id).Quantity);
        Assert.Equal("A", items[0].StoreEnName);
        Assert.Equal(4, items.Single(i => i.ProductId == flour.Id).Cost);
    }

    [Fact]
    public async Task Lookups_RolesAndHrTypes()
    {
        var sut = new LookupRepository(_db.Context);
        _db.Context.Roles.Add(new Microsoft.AspNetCore.Identity.IdentityRole("Waiter") { NormalizedName = "WAITER" });
        _db.Context.LeaveTypes.Add(new LeaveType { Code = "1", ArName = "سنوية", EnName = "Annual", DaysPerYear = 21 });
        _db.Context.AllowanceTypes.Add(new AllowanceType { Code = "1", ArName = "بدل", EnName = "Allowance", Amount = 100 });
        _db.Context.DeductionTypes.Add(new DeductionType { Code = "1", ArName = "جزاء", EnName = "Penalty", Amount = 50 });
        _db.Context.InsuranceCategories.Add(new InsuranceCategory { Code = "1", ArName = "فئة", EnName = "Category", BasicWage = 3000, EmployeePercent = 11 });
        await _db.Context.SaveChangesAsync();

        Assert.Equal("Waiter", Assert.Single(await sut.RoleLookupAsync()).Id);
        Assert.Equal(21, Assert.Single(await sut.LeaveTypeLookupAsync()).DaysPerYear);
        Assert.Equal(100, Assert.Single(await sut.AllowanceTypeLookupAsync()).Amount);
        Assert.Equal(50, Assert.Single(await sut.DeductionTypeLookupAsync()).Amount);
        Assert.Equal(11, Assert.Single(await sut.InsuranceCategoryLookupAsync()).EmployeePercent);
        Assert.Equal(1, (await sut.NamedLookupAsync<Kitchen>()).Count() + 1);
    }

    // ---------------------------------------------------------------- stock movements

    [Fact]
    public async Task StockMovements_FilterByStoreProductSourceDateAndSearch_NewestFirst()
    {
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var flour = new Product { Code = "1", ArName = "دقيق", EnName = "Flour" };
        var sugar = new Product { Code = "2", ArName = "سكر", EnName = "Sugar" };
        _db.Context.AddRange(store, flour, sugar);
        await _db.Context.SaveChangesAsync();
        _db.Context.StoreProductMovements.AddRange(
            new StoreProductMovement { StoreId = store.Id, ProductId = flour.Id, QuantityDelta = 5, Source = StockMovementSource.Invoice, ReferenceCode = "INV-1", MovementDate = new DateTime(2030, 1, 1) },
            new StoreProductMovement { StoreId = store.Id, ProductId = sugar.Id, QuantityDelta = -2, Source = StockMovementSource.Order, ReferenceCode = "ORD-9", MovementDate = new DateTime(2030, 1, 3) });
        await _db.Context.SaveChangesAsync();
        var sut = new StockMovementRepository(_db.Context);
        var wide = new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) };

        var all = await sut.GetPagedAsync(wide, null, null);
        Assert.Equal(2, all.TotalRecords);
        Assert.Equal(sugar.Id, all.Data[0].ProductId);
        Assert.Single((await sut.GetPagedAsync(wide, store.Id, flour.Id)).Data);
        Assert.Empty((await sut.GetPagedAsync(wide, 999, null)).Data);
        Assert.Equal(2, (await sut.GetPagedAsync(wide, 0, 0)).TotalRecords);

        var bySource = new PageRequest { FromDate = wide.FromDate, ToDate = wide.ToDate, Type = "Order" };
        Assert.Single((await sut.GetPagedAsync(bySource, null, null)).Data);
        var bySearch = new PageRequest { FromDate = wide.FromDate, ToDate = wide.ToDate, SearchString = "SUG" };
        Assert.Single((await sut.GetPagedAsync(bySearch, null, null)).Data);
        var narrow = new PageRequest { FromDate = new DateTime(2030, 1, 3), ToDate = new DateTime(2030, 1, 3) };
        Assert.Single((await sut.GetPagedAsync(narrow, null, null)).Data);
    }


    // ---------------------------------------------------------------- location

    [Fact]
    public async Task Location_ResolvesGovernorateAndCountry_OrNothing()
    {
        var country = new Country { ArName = "مصر", EnName = "Egypt" };
        var gov = new Governorate { ArName = "القاهرة", EnName = "Cairo", Country = country };
        var town = new Town { ArName = "مدينة", EnName = "Town", Governorate = gov };
        _db.Context.Towns.Add(town);
        await _db.Context.SaveChangesAsync();
        var sut = new LocationRepository(_db.Context);

        var found = await sut.ResolveAsync(town.Id);
        Assert.Equal(gov.Id, found.GovId);
        Assert.Equal(country.Id, found.CountryId);

        Assert.Null((await sut.ResolveAsync(null)).GovId);
        Assert.Null((await sut.ResolveAsync(999)).CountryId);
    }

    // ---------------------------------------------------------------- dashboard

    [Fact]
    public async Task Dashboard_CountsTheShiftKitchenAndTables()
    {
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com", FirstName = "Ali", LastName = "Omar" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var free = new Table { Code = "1", ArName = "T1", EnName = "T1", NumOfPersons = 4 };
        var reserved = new Table { Code = "2", ArName = "T2", EnName = "T2", NumOfPersons = 2 };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(user, store, section, free, reserved, customer);
        await _db.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _db.Context.MenuItems.Add(dish);
        _db.Context.TableReservations.Add(new TableReservation
        {
            TableId = reserved.Id, CustomerName = "Sara", PhoneNumber = "01012345678", GuestCount = 2,
            ReservationDate = DateTime.Now.AddMinutes(30), Status = ReservationStatus.Confirmed
        });
        await _db.Context.SaveChangesAsync();
        var orders = new OrderRepository(_db.Context);
        _db.Context.ChangeTracker.Clear();
        Assert.True((await orders.CreateOrderAsync(new Order { Type = OrderType.TakeAway, CustomerId = customer.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } }, user.Id)).Success);
        Assert.True((await orders.CreateOrderAsync(new Order { Type = OrderType.Hall, TableId = free.Id, CustomerId = customer.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 2 } } } }, user.Id)).Success);
        _db.Context.ChangeTracker.Clear();

        var model = await new DashboardRepository(_db.Context).GetDashboardAsync(user.Id);

        Assert.Equal("Ali Omar", model.UserName);
        Assert.Equal(1, model.AwaitingPayment);
        Assert.Equal(80, model.AwaitingPaymentAmount);
        Assert.Equal(2, model.InKitchen);
        Assert.Equal(1, model.ByType.Single(a => a.Type == OrderType.TakeAway).Paid);
        Assert.Equal(1, model.ByType.Single(a => a.Type == OrderType.Hall).Unpaid);
        Assert.Equal(0, model.WaitingReservations);
        Assert.Equal(DashboardTableState.Occupied, model.Tables.Single(a => a.Id == free.Id).State);
        var card = model.Tables.Single(a => a.Id == reserved.Id);
        Assert.Equal(DashboardTableState.Reserved, card.State);
        Assert.Equal("Sara", card.CustomerName);
        Assert.Equal(2, model.Kitchen.Count);
    }

    [Fact]
    public async Task Dashboard_EmptyDatabase_DoesNotCrash()
    {
        var model = await new DashboardRepository(_db.Context).GetDashboardAsync("nobody");

        Assert.Equal(string.Empty, model.UserName);
        Assert.Empty(model.Tables);
    }
}
