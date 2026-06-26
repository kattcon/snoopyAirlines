namespace SnoopyAirlines.Domain
{
    public class FlightQuery
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? EarliestDeparture { get; set; }
        public DateTime? LatestDeparture { get; set; }
        public DateTime? EarliestArrival { get; set; }
        public DateTime? LatestArrival { get; set; }
        public int? QuantityOfPassengers { get; set; }
        public bool IncludeStopovers { get; set; }
    }
}
