namespace ResturantPL.ViewModels
{
    public class IndexSettingsViewModel
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string StartHour { get; set; }
        public string EndHour { get; set; }
        public IFormFile Video { get; set; }
        public string VedioUrl { get; set; }
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
        public string Email { get; set; }
        public string Locaion { get; set; }

    }
}
