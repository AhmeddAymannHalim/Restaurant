using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class EmployeeAccountViewModel
{
    public string Id { get; set; }
    public int? EmployeeId { get; set; }
    public string Code { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string UserType { get; set; }
    public string PhoneNumber { get; set; }

    public string Password { get; set; }
    public string ConfirmPassword { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    // a new login that is not for one fixed employee shows the list of employees without a login
    public bool NeedsEmployeeList => string.IsNullOrEmpty(Id) && EmployeeId is null or 0;

    public IEnumerable<LookupItem> Employees { get; set; } = new List<LookupItem>();
}
