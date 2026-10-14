using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;

namespace Restaurant.DAL.Repositories.Commons;

public class ValidatorRepository<T> : IValidatorRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;

    public ValidatorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private string KeyName()
    {
        return _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0].Name;
    }

    private static string Normalize(string value)
    {
        return value.Replace(" ", string.Empty).ToLower();
    }

    public bool IsCodeExist(int id, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var keyName = KeyName();
        var normalized = Normalize(code);

        return _context.Set<T>()
            .AsNoTracking()
            .Any(e => EF.Property<string>(e, "Code").Replace(" ", "").ToLower() == normalized
                   && EF.Property<int>(e, keyName) != id);
    }

    public bool IsArNameExist(int id, string arName)
    {
        if (string.IsNullOrWhiteSpace(arName))
        {
            return false;
        }

        var keyName = KeyName();
        var normalized = Normalize(arName);

        return _context.Set<T>()
            .AsNoTracking()
            .Any(e => EF.Property<string>(e, "ArName").Replace(" ", "").ToLower() == normalized
                   && EF.Property<int>(e, keyName) != id);
    }

    public bool IsEnNameExist(int id, string enName)
    {
        if (string.IsNullOrWhiteSpace(enName))
        {
            return false;
        }

        var keyName = KeyName();
        var normalized = Normalize(enName);

        return _context.Set<T>()
            .AsNoTracking()
            .Any(e => EF.Property<string>(e, "EnName").Replace(" ", "").ToLower() == normalized
                   && EF.Property<int>(e, keyName) != id);
    }

    public bool IsPhoneNumberExist(int id, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var keyName = KeyName();

        return _context.Set<T>()
            .AsNoTracking()
            .Any(e => EF.Property<string>(e, "PhoneNumber") == phoneNumber
                   && EF.Property<int>(e, keyName) != id);
    }

    public bool IsFullNameExist(int id, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return false;
        }

        var keyName = KeyName();
        var first = Normalize(firstName);
        var last = Normalize(lastName);

        return _context.Set<T>()
            .AsNoTracking()
            .Any(e => EF.Property<string>(e, "FirstName").Replace(" ", "").ToLower() == first
                   && EF.Property<string>(e, "LastName").Replace(" ", "").ToLower() == last
                   && EF.Property<int>(e, keyName) != id);
    }

    public bool IsValueExist(int id, string propertyName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var keyName = KeyName();

        return _context.Set<T>()
            .AsNoTracking()
            .Any(e => EF.Property<string>(e, propertyName) == value
                   && EF.Property<int>(e, keyName) != id);
    }
}
