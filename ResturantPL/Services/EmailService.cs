using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ResturantPL.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_options.UserName) || string.IsNullOrWhiteSpace(_options.Password))
            {
                _logger.LogError("Email is NOT configured (set Email:UserName and Email:Password in user-secrets), so nothing was sent. To: {To} | {Subject} | {Body}",
                    to,
                    subject,
                    htmlBody);
                return;
            }

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.UserName, _options.Password)
            };
            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress ?? _options.UserName, _options.FromName),
                Subject = subject,
                Body = Wrap(subject, htmlBody),
                IsBodyHtml = true
            };
            message.To.Add(to);
            await client.SendMailAsync(message);
        }

        private static string Wrap(string title, string body) => $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;background:#1b2030;padding:32px"">
  <div style=""max-width:480px;margin:auto;background:#262c40;border-radius:16px;padding:32px;color:#e8ebf5"">
    <h2 style=""margin:0 0 16px;color:#fff"">{title}</h2>
    {body}
  </div>
</div>";
    }
}
