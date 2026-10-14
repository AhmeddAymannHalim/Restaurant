namespace Restaurant.DAL.Models;

public class Country
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    public virtual ICollection<Governorate> Governorates { get; set; }
}

public class Governorate
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int CountryId { get; set; }

    public virtual Country Country { get; set; }
    public virtual ICollection<City> Cities { get; set; }
}

public class City
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int GovernorateId { get; set; }

    public virtual Governorate Governorate { get; set; }
}
