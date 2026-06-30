namespace SnoopyAirlines.Domain.View
{
    public class FlightResponse
    {
        public IReadOnlyCollection<FlightLegResponse> Flights { get; set; } = Array.Empty<FlightLegResponse>();
    }

    public class FlightLegResponse
    {
        public int SequenceNumber { get; set; }
        public int? RouteId { get; set; }
        public string? FlightGuid { get; set; }
        public DateOnly IntendedDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
        required public string Duration { get; set; }
        required public AirportResponse DepartureAirport { get; set; }
        required public AirportResponse ArrivalAirport { get; set; }
        public decimal TouristPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
        public bool IsExternal { get; set; }
        public string? Airline { get; set; }
    }

    public class AirportResponse
    {
        required public string Code { get; set; }
        required public string Name { get; set; }
        required public string City { get; set; }
    }
}
