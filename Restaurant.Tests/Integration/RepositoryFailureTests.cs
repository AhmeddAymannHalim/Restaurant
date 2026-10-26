using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// A repository method that answers with a ResultMessage must never let a database failure escape as an exception:
// every read or save that breaks (a constraint, a dead connection) has to come back as a failed ResultMessage.
public class RepositoryFailureTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private static IEnumerable<Type> RepositoryContracts() =>
        typeof(Restaurant.DAL.Repositories.Definitions.KitchenRepository).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && t.Name.EndsWith("Repository"))
            .Select(t => t.GetInterfaces().FirstOrDefault(i => i.Name == "I" + t.Name))
            .Where(i => i != null);

    public static IEnumerable<object[]> ResultMethods() =>
        RepositoryContracts()
            .SelectMany(contract => contract.GetMethods()
                .Where(m => m.ReturnType == typeof(Task<ResultMessage>))
                // with no lines to post there is nothing to read or save, so it answers success (tested below with real lines)
                .Where(m => m.Name != "PostSourceEntryAsync")
                .Select(m => new object[] { contract.Name + "." + m.Name }));

    private static System.Reflection.MethodInfo Find(string name)
    {
        var parts = name.Split('.');
        var contract = RepositoryContracts().Single(c => c.Name == parts[0]);

        return contract.GetMethods().Single(m => m.Name == parts[1] && m.ReturnType == typeof(Task<ResultMessage>));
    }

    private async Task<ResultMessage> Run(string name, Exception failure)
    {
        var method = Find(name);
        var repository = _host.Get(RepositoryContracts().Single(c => c.Name == name.Split('.')[0]));
        _host.Db.QueryFailure.Failure = failure;
        try
        {
            return await (Task<ResultMessage>)method.Invoke(repository, Args.For(method));
        }
        finally
        {
            _host.Db.QueryFailure.Failure = null;
        }
    }

    [Theory]
    [MemberData(nameof(ResultMethods))]
    public async Task WhenTheDatabaseFails_TheAnswerIsAFailure_NotAnException(string method)
    {
        var result = await Run(method, new InvalidOperationException("forced"));

        Assert.False(result.Success, method);
        Assert.False(string.IsNullOrWhiteSpace(result.Message), method);
    }

    [Theory]
    [MemberData(nameof(ResultMethods))]
    public async Task WhenASaveBreaksAConstraint_TheAnswerIsAFailure_NotAnException(string method)
    {
        var result = await Run(method, new DbUpdateException("forced"));

        Assert.False(result.Success, method);
        Assert.False(string.IsNullOrWhiteSpace(result.Message), method);
    }

    [Theory]
    [InlineData("database")]
    [InlineData("constraint")]
    public async Task PostingAJournalEntry_WhenTheDatabaseFails_IsAFailureNotAnException(string kind)
    {
        var journals = _host.Get<Restaurant.DAL.IRepositories.Commons.IJournalEntryRepository>();
        _host.Db.QueryFailure.Failure = kind == "database" ? new InvalidOperationException("forced") : new DbUpdateException("forced");
        var lines = new[]
        {
            new JournalLineRequest(null, 5, 0),
            new JournalLineRequest(null, 0, 5)
        };

        var result = await journals.PostSourceEntryAsync(JournalSourceType.CustomerPaymentVoucher, 1, DateTime.Today, "x", lines);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }
}
