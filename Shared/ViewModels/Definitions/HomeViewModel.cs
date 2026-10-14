using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class HomeViewModel
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArDescription { get; set; }
    public string EnDescription { get; set; }
    public string Phone { get; set; }
    public string StartDay { get; set; }
    public string EndDay { get; set; }
    public string StartHour { get; set; }
    public string EndHour { get; set; }
    public string VideoUrl { get; set; }
    public string ImageUrl { get; set; }
    public string Email { get; set; }
    public string ArLocation { get; set; }
    public string EnLocation { get; set; }

    public List<LookupItem> MenuSections { get; set; } = new();
    public List<ChefViewModel> Chefs { get; set; } = new();
    public List<GalleryViewModel> Gallery { get; set; } = new();
}

public class HomeMenuItemViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArDescription { get; set; }
    public string EnDescription { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string Size { get; set; }
    public string ArIngredients { get; set; }
    public string EnIngredients { get; set; }
}
