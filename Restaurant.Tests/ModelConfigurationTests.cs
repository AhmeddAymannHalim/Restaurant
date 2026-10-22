using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Restaurant.DAL.Configurations;
using Restaurant.DAL.Models;

namespace Restaurant.Tests;

// The EF Fluent API configurations, read from the model the app builds (no database round trip needed).
public class ModelConfigurationTests : IDisposable
{
    private readonly TestDb _db = new();
    private IModel Model => _db.Context.Model;

    public void Dispose() => _db.Dispose();

    private static IEnumerable<Type> ConfigurationTypes() => typeof(KitchenConfig).Assembly.GetTypes()
        .Where(a => !a.IsAbstract && a.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));

    public static IEnumerable<object[]> Configurations() => ConfigurationTypes().Select(a => new object[] { a });

    [Theory]
    [MemberData(nameof(Configurations))]
    public void EveryConfiguration_BelongsToAnEntityInTheModel(Type configuration)
    {
        var entity = configuration.GetInterfaces().First(i => i.IsGenericType).GetGenericArguments()[0];

        var entityType = Model.FindEntityType(entity);

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void EveryEntity_HasAPrimaryKey()
    {
        var without = Model.GetEntityTypes().Where(a => !a.IsOwned() && a.FindPrimaryKey() == null).Select(a => a.ClrType.Name).ToList();

        Assert.True(without.Count == 0, "no primary key: " + string.Join(", ", without));
    }

    [Fact]
    public void EveryDecimalColumn_HasAnExplicitPrecision()
    {
        // SQL Server silently truncates a decimal to 2 places when no precision is set
        var missing = Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties().Select(p => new { Entity = e, Property = p }))
            .Where(a => (a.Property.ClrType == typeof(decimal) || a.Property.ClrType == typeof(decimal?)))
            .Where(a => a.Property.GetPrecision() == null && a.Property.GetColumnType() == null)
            .Select(a => a.Entity.ClrType.Name + "." + a.Property.Name)
            .ToList();

        Assert.True(missing.Count == 0, "decimal without precision: " + string.Join(", ", missing));
    }

    [Fact]
    public void DeletingMasterData_NeverCascadesToTheMoneyAndStockHistory()
    {
        // a cascade from these parents would silently erase invoices, payments or stock movements
        var parents = new[] { nameof(Account), nameof(Product), nameof(Store), nameof(Supplier), nameof(Customer), nameof(Employee), nameof(UnitPrice) };

        // known on purpose: an employee's photos go with the employee; movements go with a product/store, whose delete is already
        // refused while stock remains (CannotDeleteHasStock). Whether past movements should block the delete too is left to the owner.
        var known = new[] { "Employee -> EmployeeImage", "Product -> StoreProductMovement", "Store -> StoreProductMovement" };

        var cascades = Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && parents.Contains(fk.PrincipalEntityType.ClrType.Name))
            .Select(fk => fk.PrincipalEntityType.ClrType.Name + " -> " + fk.DeclaringEntityType.ClrType.Name)
            .Where(a => !known.Contains(a))
            .ToList();

        Assert.True(cascades.Count == 0, "cascade delete from master data: " + string.Join(", ", cascades));
    }

    [Fact]
    public void Kitchen_NamesAreRequiredUniqueAndLimitedTo100()
    {
        var kitchen = Model.FindEntityType(typeof(Kitchen));

        foreach (var name in new[] { nameof(Kitchen.ArName), nameof(Kitchen.EnName) })
        {
            var property = kitchen.FindProperty(name);
            Assert.False(property.IsNullable);
            Assert.Equal(100, property.GetMaxLength());
            Assert.Contains(kitchen.GetIndexes(), i => i.IsUnique && i.Properties.Single() == property);
        }
    }

    [Fact]
    public void MenuItemComponent_DeletesWithItsDishButKeepsTheProduct()
    {
        var component = Model.FindEntityType(typeof(MenuItemComponent));

        var dish = component.GetForeignKeys().Single(a => a.PrincipalEntityType.ClrType == typeof(MenuItem));
        var product = component.GetForeignKeys().Single(a => a.PrincipalEntityType.ClrType == typeof(Product));

        Assert.Equal(DeleteBehavior.Cascade, dish.DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, product.DeleteBehavior);
        Assert.Equal(3, component.FindProperty(nameof(MenuItemComponent.Quantity)).GetScale());
    }

    [Fact]
    public void StockQuantities_KeepThreeDecimalPlaces()
    {
        var quantity = Model.FindEntityType(typeof(StoreItemUnit)).FindProperty(nameof(StoreItemUnit.Quantity));

        Assert.Equal(18, quantity.GetPrecision());
        Assert.Equal(3, quantity.GetScale());
    }
}
