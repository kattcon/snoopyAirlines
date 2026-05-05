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
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Email queued for console delivery.\nTo: {To}\nSubject: {Subject}\nBody:\n{Body}",
                to,
                subject,
                body);

            return Task.CompletedTask;
        }
    }
}
