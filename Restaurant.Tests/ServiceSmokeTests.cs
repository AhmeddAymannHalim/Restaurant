using System.Reflection;

namespace Restaurant.Tests;

// Every public method of every service, called through the real graph on an empty database:
// it must answer (a result, null, an empty list) and never throw.
public class ServiceSmokeTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private static IEnumerable<Type> ServiceContracts() => typeof(Restaurant.BLL.Services.Definitions.KitchenService).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service") && !t.IsGenericTypeDefinition)
        .Select(t => t.GetInterfaces().FirstOrDefault(i => i.Name == "I" + t.Name))
        .Where(i => i != null);

    public static IEnumerable<object[]> Methods() => ServiceContracts()
        .SelectMany(contract => contract.GetMethods().Select(m => new object[] { contract.Name + "." + m.Name + "/" + m.GetParameters().Length }));

    private static (Type contract, MethodInfo method) Find(string key)
    {
        var parts = key.Split('.', '/');
        var contract = ServiceContracts().Single(c => c.Name == parts[0]);
        var method = contract.GetMethods().Single(m => m.Name == parts[1] && m.GetParameters().Length == int.Parse(parts[2]));

        return (contract, method);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    public async Task EveryServiceMethod_AnswersOnAnEmptyDatabase(string key)
    {
        var (contract, method) = Find(key);
        var service = _host.Get(contract);
        var target = method.IsGenericMethodDefinition ? method.MakeGenericMethod(typeof(Restaurant.DAL.Models.Kitchen)) : method;

        var result = target.Invoke(service, Args.For(target));
        if (result is Task task)
        {
            await task;
        }

        Assert.True(true);
    }
}
