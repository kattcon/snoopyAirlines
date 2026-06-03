namespace SnoopyAirlines.Services
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken,
            bool isHtml = false)
        {
            _logger.LogInformation(
                "Email queued for console delivery.\nTo: {To}\nSubject: {Subject}\nIsHtml: {IsHtml}\nBody:\n{Body}",
                to,
                subject,
                isHtml,
                body);

            return Task.CompletedTask;
        }
    }
}
