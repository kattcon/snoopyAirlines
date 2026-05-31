using System.Net;
using System.Net.Mail;
using System.Text;
using System.Net.Mime;

namespace SnoopyAirlines.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken,
            bool isHtml = false)
        {
            var host = GetRequiredSetting("Email:Host");
            var fromAddress = GetRequiredSetting("Email:FromAddress");
            var fromName = _configuration["Email:FromName"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var port = GetPort();
            var enableSsl = GetEnableSsl();

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            message.To.Add(to);

            if (isHtml)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Html);
                var logoPath = Path.Combine(AppContext.BaseDirectory, "frontend", "src","assets", "logoSA.png");

                if (File.Exists(logoPath))
                {
                    var logo = new LinkedResource(logoPath, MediaTypeNames.Image.Png)
                    {
                        ContentId = "logoSA",
                        TransferEncoding = TransferEncoding.Base64
                    };

                    logo.ContentType.Name = "logoSA.png";
                    htmlView.LinkedResources.Add(logo);
                }

                message.AlternateViews.Add(htmlView);
            }
            else
            {
                message.Body = body;
                message.IsBodyHtml = false;
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            await client.SendMailAsync(message, cancellationToken);
        }

        private string GetRequiredSetting(string key)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Email setting '{key}' is required.");
            }

            return value;
        }

        private int GetPort()
        {
            var configuredPort = _configuration["Email:Port"];

            if (int.TryParse(configuredPort, out var port))
            {
                return port;
            }

            return 587;
        }

        private bool GetEnableSsl()
        {
            var configuredEnableSsl = _configuration["Email:EnableSsl"];

            if (bool.TryParse(configuredEnableSsl, out var enableSsl))
            {
                return enableSsl;
            }

            return true;
        }
    }
}
