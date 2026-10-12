namespace Shared.ViewModels.Definitions;

// Everything the public home page shows - all of it is managed from the dashboard.
public class WebsiteViewModel
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; }
    public string StartDay { get; set; }
    public string EndDay { get; set; }
    public string StartHour { get; set; }
    public string EndHour { get; set; }
    public string VedioUrl { get; set; }
    public string ImageUrl { get; set; }
    public string Email { get; set; }
    public string Locaion { get; set; }

    public List<WebsiteMenuSectionViewModel> MenuSections { get; set; } = new();
    public List<ChefViewModel> Chefs { get; set; } = new();
    public List<GalleryViewModel> Gallery { get; set; } = new();
}

public class WebsiteMenuSectionViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
}

// Returned by the AJAX call that fills the menu when a category is picked.
public class WebsiteMenuItemViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string Size { get; set; }
}
