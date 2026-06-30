using System.Text.Json.Serialization;
using SnoopyAirlines.External.Domain.Intake;

namespace SnoopyAirlines.External.Domain.View
{
    public class ExternalOrderResponse
    {
        public string ReservationNumber { get; set; } = string.Empty;
        public bool FirstClass { get; set; }
        required public Flight Flight { get; set; }
        required public ExternalOrderBreakup Breakup { get; set; }

        required public IReadOnlyCollection<ExternalOrderPassenger> Passengers { get; set; }

        required public ExternalOrderBuyerResponse Buyer { get; set; }
    }

    public class ExternalOrderBreakup
    {
        public decimal Luggage { get; set; }
        public decimal Tickets { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
    }

    public class ExternalOrderBuyerResponse
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        public string? LastName2 { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ExternalOrderErrorResponse
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FieldName { get; set; }
    }
}
