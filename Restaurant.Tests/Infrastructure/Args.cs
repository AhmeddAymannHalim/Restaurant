using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Restaurant.Tests;

// Makes harmless arguments for any method: unknown ids, empty lists, empty view models.
public static class Args
{
    public static object[] For(MethodInfo method) => method.GetParameters().Select(p => Make(p.ParameterType, p.Name)).ToArray();

    public static object Make(Type type, string name = null)
    {
        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable != null)
        {
            return null;
        }

        if (type == typeof(string)) return name != null && name.Contains("mail", StringComparison.OrdinalIgnoreCase) ? "nobody@test.com" : "x";
        if (type == typeof(int)) return 999;
        if (type == typeof(long)) return 999L;
        if (type == typeof(decimal)) return 1m;
        if (type == typeof(double)) return 1d;
        if (type == typeof(bool)) return false;
        if (type == typeof(DateTime)) return DateTime.Today;
        if (type == typeof(Guid)) return Guid.NewGuid();
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
        if (typeof(IFormFile).IsAssignableFrom(type)) return null;
        if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>)) return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()));
        if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()));
        if (type.IsClass && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
        {
            // a view model as a user would post it: its text fields are filled, never null
            var instance = Activator.CreateInstance(type);
            foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanWrite && p.GetIndexParameters().Length == 0))
            {
                if (property.GetValue(instance) == null)
                {
                    property.SetValue(instance, property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase) ? "nobody@test.com" : "x");
                }
            }

            return instance;
        }
        if (type.IsValueType) return Activator.CreateInstance(type);

        return null;
    }
}
