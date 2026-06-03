using System.Reflection;
using snoopy_airlines_backend.Domain;

namespace SnoopyAirlines.Domain.EmailTemplate
{
    public static class BookingConfirmationEmail
    {
        private const string ResourceName = "snoopy_airlines_backend.Resources.Emails.bookingConfirmationEmail.html";

        public static string Build(PurchaseOrderEmailData data)
        {
            var html = LoadTemplate(ResourceName);

            return html
                .Replace("{{PURCHASE_ORDER_ID}}", data.PurchaseOrderId.ToString())
                .Replace("{{BASE_FARE}}", data.BaseFare)
                .Replace("{{TAXES}}", data.Taxes)
                .Replace("{{TRAVEL_INSURANCE}}", data.TravelInsurance)
                .Replace("{{TOTAL}}", data.Total)
                .Replace("{{PAYMENT_METHOD}}", data.PaymentMethod)
                .Replace("{{PURCHASE_DATE}}", data.PurchaseDate)
                .Replace("{{TRANSACTION_ID}}", data.TransactionId);
        }

        private static string LoadTemplate(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Email template not found as embedded resource: '{resourceName}'. " +
                    $"Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
