namespace SnoopyAirlines.Util.Email
{
    public interface IEmailSender
    {
        Task SendAsync<T>(
            string to,
            IEmailTemplate<T> template,
            T data,
            CancellationToken cancellationToken = default);

        Task SendAsync<T>(
            string to,
            IEmailTemplate<T> template,
            T data,
            Attachment attachment,
            CancellationToken cancellationToken = default);
    }
}
