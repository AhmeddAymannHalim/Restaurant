namespace ResturantPL.ViewModels
{
    public class IndexSettingsViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string StartHour { get; set; }
        public string EndHour { get; set; }
        public string VedioUrl { get; set; }
        public IFormFile Image { get; set; }
        public string Email { get; set; }
        public string Locaion { get; set; }

    }
}
