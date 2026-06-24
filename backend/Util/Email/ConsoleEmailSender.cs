namespace SnoopyAirlines.Util.Email
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync<T>(
            string to,
            IEmailTemplate<T> template,
            T data,
            CancellationToken cancellationToken = default)
        {
            return SendRenderedAsync(to, template, data, attachment: null, cancellationToken);
        }

        public Task SendAsync<T>(
            string to,
            IEmailTemplate<T> template,
            T data,
            Attachment attachment,
            CancellationToken cancellationToken = default)
        {
            return SendRenderedAsync(to, template, data, attachment, cancellationToken);
        }

        private Task SendRenderedAsync<T>(
            string to,
            IEmailTemplate<T> template,
            T data,
            Attachment? attachment,
            CancellationToken cancellationToken)
        {
            var email = EmailTemplateRenderer.Render(template, data);

            _logger.LogInformation(
                "Email queued for console delivery.\nTo: {To}\nSubject: {Subject}\nTemplate: {Template}\nAttachment: {Attachment}\nBody:\n{Body}",
                to,
                email.Subject,
                template.TemplateName,
                attachment?.FileName ?? "none",
                email.Body);

            return Task.CompletedTask;
        }
    }
}
