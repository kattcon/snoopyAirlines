using snoopy_airlines_backend.Domain;

namespace SnoopyAirlines.Util.Email.Templates
{
    public sealed class BookingConfirmationEmail : IEmailTemplate<PurchaseOrderEmailData>
    {
        public string TemplateName => "snoopy_airlines_backend.Resources.Emails.bookingConfirmationEmail.html";
        public string Subject => "Confirmaci\u00f3n de reserva - Snoopy Airlines";

        public IReadOnlyList<EmailTemplateParameter> GetParameters(PurchaseOrderEmailData data)
        {
            return
            [
                new EmailTemplateParameter("PURCHASE_ORDER_ID", data.ConfirmationCode.ToString()),
                new EmailTemplateParameter("BASE_FARE", data.BaseFare),
                new EmailTemplateParameter("TAXES", data.Taxes),
                new EmailTemplateParameter("TRAVEL_INSURANCE", data.TravelInsurance),
                new EmailTemplateParameter("TOTAL", data.Total),
                new EmailTemplateParameter("PAYMENT_METHOD", data.PaymentMethod),
                new EmailTemplateParameter("PURCHASE_DATE", data.PurchaseDate),
                new EmailTemplateParameter("TRANSACTION_ID", data.TransactionId)
            ];
        }
    }
}
