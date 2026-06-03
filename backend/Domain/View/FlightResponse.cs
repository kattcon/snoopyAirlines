namespace SnoopyAirlines.Domain.View
{
    public class FlightResponse
    {
        required public string FlightGUID { get; set; }
        public int RouteId { get; set; }
        public IReadOnlyCollection<FlightRouteResponse> Routes { get; set; } = Array.Empty<FlightRouteResponse>();
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        required public string Duration { get; set; }
        required public AirportResponse DepartureAirport { get; set; }
        required public AirportResponse ArrivalAirport { get; set; }
        public bool HasStopover { get; set; }
        public AirportResponse? StopoverAirport { get; set; }
        public string? StopoverDuration { get; set; }
        public decimal TouristPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
    }

    public class FlightRouteResponse
    {
        public int SequenceNumber { get; set; }
        public int RouteId { get; set; }
        public DateOnly IntendedDate { get; set; }
    }

    public class AirportResponse
    {
        required public string Code { get; set; }
        required public string Name { get; set; }
        required public string City { get; set; }
    }
}
