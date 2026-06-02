namespace SnoopyAirlines.Domain
{
    public class RouteQuery
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? EarliestDeparture { get; set; }
        public DateTime? LatestDeparture { get; set; }
        public int? QuantityOfPassengers { get; set; }
    }
}
