namespace Shared.ViewModels.Definitions;

public class GalleryViewModel
{
    public int Id { get; set; }
    public string PictureUrl { get; set; }
    public IFormFile ImageFile { get; set; }
}
