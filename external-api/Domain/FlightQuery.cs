namespace SnoopyAirlines.External.Domain
{
    public class FlightQuery
    {
        public string? Destination { get; set; }
        public DateTime? EarliestArrival { get; set; }
        public DateTime? LatestArrival { get; set; }
        public int? QuantityOfPassengers { get; set; }
    }
}
