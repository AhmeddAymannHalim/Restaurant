namespace Shared.ViewModels.Definitions;

public class EmployeeViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string MobileNumber { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string NationalID { get; set; }
    public byte Age { get; set; }
    public int SalaryTypeId { get; set; }
    public decimal Salary { get; set; }
    public int BrancheId { get; set; }
    public int JobId { get; set; }
    public string UserType { get; set; }
    public string Image { get; set; }
    public IFormFile ImageFile { get; set; }

    // Index columns
    public string JobArName { get; set; }
    public string JobEnName { get; set; }
    public string BranchArName { get; set; }
    public string BranchEnName { get; set; }
    public string SalaryTypeArName { get; set; }
    public string SalaryTypeEnName { get; set; }
    public bool HasAccount { get; set; }

    // Lookups for the Upsert form
    public SelectList Branches { get; set; }
    public SelectList Jobs { get; set; }
    public SelectList SalaryTypes { get; set; }
}
