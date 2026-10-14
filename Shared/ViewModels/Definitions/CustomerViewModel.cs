namespace Shared.ViewModels.Definitions;

public class CustomerViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public decimal Balance { get; set; }
    public int? CityId { get; set; }

    public string CityArName { get; set; }
    public string CityEnName { get; set; }

    /// <summary>Not persisted — used only by the Country/Governorate AJAX cascade to preselect on edit.</summary>
    public int? CountryId { get; set; }
    /// <summary>Not persisted — used only by the Governorate AJAX cascade to preselect on edit.</summary>
    public int? GovernorateId { get; set; }
}
