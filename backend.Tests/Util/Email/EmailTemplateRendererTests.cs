using SnoopyAirlines.Util.Email;
using Xunit;

namespace backend.Tests.Util.Email
{
    public class EmailTemplateRendererTests
    {
        [Fact]
        public void TestRenderReplacesParametersInSubjectAndBody()
        {
            // Arrange
            var template = new TestEmailTemplate();
            var data = new TestEmailData(
                "Jane",
                "https://snoopyairlines.com/register?key=abc123");

            // Act
            var result = EmailTemplateRenderer.Render(template, data);

            // Assert
            Assert.Equal("Welcome Jane", result.Subject);
            Assert.Contains(data.RegistrationUrl, result.Body);
        }

        private sealed record TestEmailData(string Name, string RegistrationUrl);

        private sealed class TestEmailTemplate : IEmailTemplate<TestEmailData>
        {
            public string TemplateName => "snoopy_airlines_backend.Resources.Emails.registrationEmail.html";
            public string Subject => "Welcome {{NAME}}";

            public IReadOnlyList<EmailTemplateParameter> GetParameters(TestEmailData data)
            {
                return
                [
                    new EmailTemplateParameter("NAME", data.Name),
                    new EmailTemplateParameter("REGISTRATION_URL", data.RegistrationUrl)
                ];
            }
        }
    }
}
