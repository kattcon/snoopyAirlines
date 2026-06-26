namespace SnoopyAirlines.Util.Email.Templates
{
    public sealed class RegistrationEmailTemplate : IEmailTemplate<RegistrationEmailData>
    {
        public string TemplateName => "snoopy_airlines_backend.Resources.Emails.registrationEmail.html";
        public string Subject => "Bienvenido a Snoopy Airlines";

        public IReadOnlyList<EmailTemplateParameter> GetParameters(RegistrationEmailData data)
        {
            return
            [
                new EmailTemplateParameter("REGISTRATION_URL", data.RegistrationUrl)
            ];
        }
    }
}
