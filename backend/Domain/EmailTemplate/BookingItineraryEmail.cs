using System.Reflection;
using System.Text;
using snoopy_airlines_backend.Domain;

namespace SnoopyAirlines.Domain.EmailTemplate
{
    public static class BookingItineraryEmail
    {
        private const string ResourceName = "snoopy_airlines_backend.Resources.Emails.itineraryEmail.html";

        public static string Build(PurchaseOrderEmailData data)
        {
            var html = LoadTemplate(ResourceName);
            var passengerHtml = BuildPassengerHtml(data.Passengers);

            return html
                .Replace("{{CONFIRMATION_CODE}}", data.ConfirmationCode.ToString())
                .Replace("{{DEPARTURE_CITY}}", data.DepartureCityName)
                .Replace("{{DEPARTURE_CODE}}", data.DepartureAirportCode)
                .Replace("{{DEPARTURE_AIRPORT}}", data.DepartureAirportName)
                .Replace("{{ARRIVAL_CITY}}", data.ArrivalCityName)
                .Replace("{{ARRIVAL_CODE}}", data.ArrivalAirportCode)
                .Replace("{{ARRIVAL_AIRPORT}}", data.ArrivalAirportName)
                .Replace("{{DEPARTURE_DATE}}", data.DepartureAt.ToString("dd MMM yyyy"))
                .Replace("{{DEPARTURE_TIME}}", data.DepartureAt.ToString("hh:mm tt"))
                .Replace("{{ARRIVAL_TIME}}", data.ArrivalAt.ToString("hh:mm tt"))
                .Replace("{{SEAT_CLASS}}", data.SeatClass)
                .Replace("{{PASSENGERS_HTML}}", passengerHtml);
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
