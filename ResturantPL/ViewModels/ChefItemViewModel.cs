namespace ResturantPL.ViewModels
{
    public class ChefItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Jop { get; set; }
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
    }
}
