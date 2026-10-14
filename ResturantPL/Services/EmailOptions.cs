namespace ResturantPL.Services
{
    public class EmailOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; } = "Restaurant";
    }
}
