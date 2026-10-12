using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class EmployeeAccountViewModel
{
    public string Id { get; set; }
    public int EmployeeId { get; set; }
    public string Code { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string UserType { get; set; }
    public string PhoneNumber { get; set; }
    public string MobileNumber { get; set; }

    public string Password { get; set; }
    public string ConfirmPassword { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    public IEnumerable<LookupItem> Employees { get; set; } = new List<LookupItem>();
}
