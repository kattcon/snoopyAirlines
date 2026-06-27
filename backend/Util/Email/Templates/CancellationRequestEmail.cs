namespace SnoopyAirlines.Util.Email.Templates
{
    public sealed class CancellationRequestEmail : IEmailTemplate<CancellationRequestEmailData>
    {
        public string TemplateName =>
            "snoopy_airlines_backend.Resources.Emails.cancelReservationEmail.html";

        public string Subject =>
            "Solicitud de cancelaci\u00f3n de reservaci\u00f3n \u2014 Snoopy Airlines";

        public IReadOnlyList<EmailTemplateParameter> GetParameters(CancellationRequestEmailData data)
        {
            return
            [
                new EmailTemplateParameter("CONFIRMATION_CODE", data.ConfirmationCode),
                new EmailTemplateParameter("CANCEL_URL",        data.CancelUrl)
            ];
        }
    }
}