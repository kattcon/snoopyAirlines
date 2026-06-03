namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrderEmailData
    {
        public Guid BookingGuid { get; set; }
        public string ConfirmationCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CardBrand { get; set; } = string.Empty;
        public string CardLastFour { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ConfirmedAt { get; set; }

        public int PurchaseOrderId { get; set; }
        public string SeatClass { get; set; } = string.Empty;

        public string DepartureAirportName { get; set; } = string.Empty;
        public string DepartureAirportCode { get; set; } = string.Empty;
        public string DepartureCityName { get; set; } = string.Empty;
        public DateTime DepartureAt { get; set; }

        public string ArrivalAirportName { get; set; } = string.Empty;
        public string ArrivalAirportCode { get; set; } = string.Empty;
        public string ArrivalCityName { get; set; } = string.Empty;
        public DateTime ArrivalAt { get; set; }

        public int TotalItineraryMinutes { get; set; }
        public int TotalFlightMinutes { get; set; }
        public int LayoverCount { get; set; }

        public List<PassengerEmailData> Passengers { get; set; } = [];

        // Campos calculados, se llenan en ApplyBookingDetails
        public string BaseFare { get; set; } = string.Empty;
        public string Taxes { get; set; } = string.Empty;
        public string TravelInsurance { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PurchaseDate { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }
}