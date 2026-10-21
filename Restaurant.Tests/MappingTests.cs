using AutoMapper;
using AutoMapper.Internal;
using Microsoft.Extensions.Logging.Abstractions;

namespace Restaurant.Tests;

// Strict AssertConfigurationIsValid is not used: audit fields, lookup lists and ERP-only columns are left unmapped on purpose.
// Instead, every mapping must carry same-named simple values across unchanged (members with an explicit Ignore or MapFrom are skipped).
public class MappingTests
{
    private static readonly MapperConfiguration Config =
        new(cfg => cfg.AddProfile<Restaurant.BLL.Helper.AutoMapper>(), NullLoggerFactory.Instance);

    public static IEnumerable<object[]> Maps() => Config.Internal().GetAllTypeMaps()
        .Where(m => m.SourceType.GetConstructor(Type.EmptyTypes) != null && m.DestinationType.GetConstructor(Type.EmptyTypes) != null)
        .Select(m => new object[] { m.SourceType, m.DestinationType });

    private static bool IsSimple(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
    }

    private static object Sample(Type type, int seed)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type == typeof(string)) return "v" + seed;
        if (type == typeof(bool)) return true;
        if (type == typeof(DateTime)) return new DateTime(2030, 1, 1).AddDays(seed);
        if (type.IsEnum) return Enum.GetValues(type).GetValue(Enum.GetValues(type).Length - 1);
        return Convert.ChangeType(seed % 100 + 1, type);
    }

    [Theory]
    [MemberData(nameof(Maps))]
    public void Map_SameNamedSimpleValues_ArriveUnchanged(Type sourceType, Type destinationType)
    {
        var mapper = Config.CreateMapper();
        var source = Activator.CreateInstance(sourceType);
        var seed = 1;
        foreach (var property in sourceType.GetProperties().Where(p => p.CanWrite && p.GetIndexParameters().Length == 0 && IsSimple(p.PropertyType)))
        {
            property.SetValue(source, Sample(property.PropertyType, seed++));
        }

        var destination = mapper.Map(source, sourceType, destinationType);
        var ignored = Config.Internal().GetAllTypeMaps()
            .Single(m => m.SourceType == sourceType && m.DestinationType == destinationType)
            .PropertyMaps.Where(p => p.Ignored || p.CustomMapExpression != null).Select(p => p.DestinationName).ToList();

        foreach (var from in sourceType.GetProperties().Where(p => p.CanRead && IsSimple(p.PropertyType)))
        {
            var to = destinationType.GetProperty(from.Name);
            if (to == null || !to.CanWrite || to.PropertyType != from.PropertyType || ignored.Contains(to.Name)) continue;
            Assert.True(Equals(from.GetValue(source), to.GetValue(destination)),
                $"{sourceType.Name} -> {destinationType.Name}: {from.Name} was {from.GetValue(source)} but arrived as {to.GetValue(destination)}");
        }
    }
}
