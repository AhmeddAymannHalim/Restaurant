using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Repositories.Definitions;

public class PhoneRepository : IPhoneRepository
{
    private readonly ApplicationDbContext _context;

    public PhoneRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<string> CleanNumbers(string mainNumber, IEnumerable<IPhoneRow> extraNumbers)
    {
        var main = mainNumber?.Trim();
        var numbers = new List<string>();

        if (extraNumbers == null)
        {
            return numbers;
        }

        foreach (var extra in extraNumbers)
        {
            var number = extra.PhoneNumber?.Trim();
            if (string.IsNullOrEmpty(number) || number == main || numbers.Contains(number))
            {
                continue;
            }

            numbers.Add(number);
        }

        return numbers;
    }

    public void SyncSupplierPhones(ICollection<SupplierPhone> current, List<string> wanted)
    {
        var toRemove = new List<SupplierPhone>();
        foreach (var phone in current)
        {
            if (!wanted.Contains(phone.PhoneNumber))
            {
                toRemove.Add(phone);
            }
        }

        foreach (var phone in toRemove)
        {
            _context.Remove(phone);
        }

        foreach (var number in wanted)
        {
            var exists = false;
            foreach (var phone in current)
            {
                if (phone.PhoneNumber == number)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                current.Add(new SupplierPhone { PhoneNumber = number });
            }
        }
    }
}
