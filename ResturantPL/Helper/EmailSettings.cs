
using Restaurant.DAL.Models;
using System.Net;
using System.Net.Mail;

namespace ResturantPL.Helper
{
	public class EmailSettings
	{
		public static void SendEmail(Email email)
		{
			var client = new SmtpClient("smtp.gmail.com", 587);
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential("mohamed19216811@gmail.com", "ukgo oipk bsmm atgv");
			client.Send("mohamed19216811@gmail.com", email.To, email.Title , email.Body);
		}
	}
}
