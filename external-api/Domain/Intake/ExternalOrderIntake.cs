using System.Text.Json.Serialization;

namespace SnoopyAirlines.External.Domain.Intake
{
    public class ExternalOrderIntake
    {
        public string? ApiKey { get; set; }

        [JsonPropertyName("flightGUID")]
        public string? FlightGuid { get; set; }

        public bool? FirstClass { get; set; }

        public List<ExternalOrderPassenger>? Passengers { get; set; }

        public ExternalOrderBuyer? Buyer { get; set; }

        public ExternalOrderPayment? Payment { get; set; }
    }

    public class ExternalOrderPassenger
    {
        public bool? CarryOn { get; set; }
        public int? Checked { get; set; }
        public string? Passport { get; set; }
        public string? PassportExpirationDate { get; set; }

        public string? PassportCountry { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? LastName2 { get; set; }
        public string? Gender { get; set; }
        public string? BirthDate { get; set; }
    }

    public class ExternalOrderBuyer
    {
        public string? Nationality { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? LastName2 { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class ExternalOrderPayment
    {
        public string? CardNumber { get; set; }

        public string? CardExpiration { get; set; }

        public string? Cvv { get; set; }
        public string? CardHolderName { get; set; }
    }
}
