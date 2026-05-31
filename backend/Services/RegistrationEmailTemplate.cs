using System.Reflection;

namespace SnoopyAirlines.Services
{
    public class RegistrationEmailTemplate
    {
        private const string ResourceName = "backend.resources.emails.registrationEmail.html";

        public static string Build(string registrationUrl)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(ResourceName);
                ?? throw new InvalidOperationException(
                    $"Email template not found as embedded resource: '{ResourceName}'. " +
                    $"Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");

            using var reader = new StreamReader(stream);
            var html = reader.ReadToEnd();

            return html.Replace("{{REGISTRATION_URL}}", registrationUrl);
        }
    }
}

