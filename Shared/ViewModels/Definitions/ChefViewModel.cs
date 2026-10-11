namespace Shared.ViewModels.Definitions;

public class ChefViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Jop { get; set; }
    public string ImageUrl { get; set; }
    public IFormFile ImageFile { get; set; }
}
