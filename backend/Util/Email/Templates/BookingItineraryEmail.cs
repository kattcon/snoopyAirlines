using System.Text;
using snoopy_airlines_backend.Domain;

namespace SnoopyAirlines.Util.Email.Templates
{
    public sealed class BookingItineraryEmail : IEmailTemplate<PurchaseOrderEmailData>
    {
        public string TemplateName => "snoopy_airlines_backend.Resources.Emails.itineraryEmail.html";
        public string Subject => "Itinerario de viaje - Snoopy Airlines";

        public IReadOnlyList<EmailTemplateParameter> GetParameters(PurchaseOrderEmailData data)
        {
            return
            [
                new EmailTemplateParameter("CONFIRMATION_CODE", data.ConfirmationCode.ToString()),
                new EmailTemplateParameter("DEPARTURE_CITY", data.DepartureCityName),
                new EmailTemplateParameter("DEPARTURE_CODE", data.DepartureAirportCode),
                new EmailTemplateParameter("DEPARTURE_AIRPORT", data.DepartureAirportName),
                new EmailTemplateParameter("ARRIVAL_CITY", data.ArrivalCityName),
                new EmailTemplateParameter("ARRIVAL_CODE", data.ArrivalAirportCode),
                new EmailTemplateParameter("ARRIVAL_AIRPORT", data.ArrivalAirportName),
                new EmailTemplateParameter("DEPARTURE_DATE", data.DepartureAt.ToString("dd MMM yyyy")),
                new EmailTemplateParameter("DEPARTURE_TIME", data.DepartureAt.ToString("hh:mm tt")),
                new EmailTemplateParameter("ARRIVAL_TIME", data.ArrivalAt.ToString("hh:mm tt")),
                new EmailTemplateParameter("SEAT_CLASS", data.SeatClass),
                new EmailTemplateParameter("PASSENGERS_HTML", BuildPassengerHtml(data.Passengers))
            ];
        }

        private static string BuildPassengerHtml(IEnumerable<PassengerEmailData> passengers)
        {
            var sb = new StringBuilder();

            foreach (var passenger in passengers)
            {
                sb.Append($"""
                    <table cellpadding="0" cellspacing="0" style="width:100%;border:1px solid #e5e7eb;border-radius:8px;margin-bottom:12px;">
                      <tr>
                        <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;" width="50%">
                          <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Nombre completo</p>
                          <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">{passenger.FirstName} {passenger.LastName}</p>
                        </td>
                        <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;border-left:1px solid #e5e7eb;" width="50%">
                          <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Genero</p>
                          <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">{passenger.Gender}</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 20px;" width="50%">
                          <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Fecha de nacimiento</p>
                          <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">{passenger.BirthDay} {passenger.BirthMonth} {passenger.BirthYear}</p>
                        </td>
                        <td style="padding:16px 20px;border-left:1px solid #e5e7eb;" width="50%">
                          <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Nacionalidad</p>
                          <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">{passenger.Nationality}</p>
                        </td>
                      </tr>
                    </table>
                    """);
            }

            return sb.ToString();
        }
    }
}
