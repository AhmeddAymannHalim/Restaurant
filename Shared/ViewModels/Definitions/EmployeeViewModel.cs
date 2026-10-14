using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class EmployeeViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public int CityId { get; set; }
    public string Street { get; set; }

    /// <summary>Not persisted — used only to preselect the Country/Governorate dropdowns when editing.</summary>
    public int? CountryId { get; set; }
    /// <summary>Not persisted — used only to preselect the Governorate dropdown when editing.</summary>
    public int? GovernorateId { get; set; }
    public string NationalID { get; set; }
    public byte Age { get; set; }
    public int SalaryTypeId { get; set; }
    public decimal Salary { get; set; }
    public int BrancheId { get; set; }
    public int JobId { get; set; }
    public string UserType { get; set; }
    public string Image { get; set; }
    public IFormFile ImageFile { get; set; }

    public string JobArName { get; set; }
    public string JobEnName { get; set; }
    public string BranchArName { get; set; }
    public string BranchEnName { get; set; }
    public string SalaryTypeArName { get; set; }
    public string SalaryTypeEnName { get; set; }
    public string CityArName { get; set; }
    public string CityEnName { get; set; }
    public bool HasAccount { get; set; }

    public IEnumerable<LookupItem> Branches { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Jobs { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> SalaryTypes { get; set; } = new List<LookupItem>();
}
