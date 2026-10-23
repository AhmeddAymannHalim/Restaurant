using System.Linq.Expressions;
using System.Reflection;
using Moq;

namespace Restaurant.Tests;

// Sets up service methods by name through reflection, so one test can run against every controller of the same shape.
public static class ServiceMocks
{
    public static bool IsAsyncOf(MethodInfo method, string prefix) =>
        method.Name.StartsWith(prefix) && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

    // every method of the service whose name matches answers with the given value
    public static void Answer(Mock service, Func<MethodInfo, bool> matches, Func<Type, object> value)
    {
        var serviceType = service.GetType().GetGenericArguments()[0];
        foreach (var method in serviceType.GetMethods().Where(matches))
        {
            var parameter = Expression.Parameter(serviceType, "s");
            var arguments = method.GetParameters()
                .Select(p => (Expression)Expression.Call(typeof(It), nameof(It.IsAny), new[] { p.ParameterType }))
                .ToArray();
            var lambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(serviceType, method.ReturnType), Expression.Call(parameter, method, arguments), parameter);
            var setup = service.GetType().GetMethods()
                .First(m => m.Name == "Setup" && m.IsGenericMethod && m.GetParameters()[0].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(method.ReturnType)
                .Invoke(service, new object[] { lambda });

            var inner = method.ReturnType.GetGenericArguments().Single();
            var task = typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(inner).Invoke(null, new[] { value(inner) });
            setup.GetType().GetMethod("Returns", new[] { method.ReturnType }).Invoke(setup, new[] { task });
        }
    }

    // the named method answers with the value; every other call on a loose mock keeps answering null
    public static void Answer(Mock service, string methodName, object value) =>
        Answer(service, m => m.Name == methodName, _ => value);

    // the named method throws, as a broken database would
    public static void Throw(Mock service, string methodName)
    {
        var serviceType = service.GetType().GetGenericArguments()[0];
        foreach (var method in serviceType.GetMethods().Where(m => m.Name == methodName))
        {
            var parameter = Expression.Parameter(serviceType, "s");
            var arguments = method.GetParameters()
                .Select(p => (Expression)Expression.Call(typeof(It), nameof(It.IsAny), new[] { p.ParameterType }))
                .ToArray();
            var lambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(serviceType, method.ReturnType), Expression.Call(parameter, method, arguments), parameter);
            var setup = service.GetType().GetMethods()
                .First(m => m.Name == "Setup" && m.IsGenericMethod && m.GetParameters()[0].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(method.ReturnType)
                .Invoke(service, new object[] { lambda });
            setup.GetType().GetMethod("Throws", new[] { typeof(Exception) }).Invoke(setup, new object[] { new InvalidOperationException("forced") });
        }
    }
}
