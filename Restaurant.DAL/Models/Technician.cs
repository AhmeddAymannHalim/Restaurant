
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Technician:BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string IdNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ImageUrl { get; set; }
    public GenderType? Gender { get; set; }
    public int? SpecializationId { get; set; }
    public virtual Specialization Specialization { get; set; }
    public int? CountryId { get; set; }
    public virtual Country Country { get; set; }
    public int? GovId { get; set; }
    public virtual Governorate Gov { get; set; }
    public int? TownId { get; set; }
    public virtual Town Town { get; set; }
}
