namespace SnoopyAirlines.External.Domain
{
    public class RouteQuery
    {
        public string? Destination { get; set; }
        public DateTime? EarliestArrival { get; set; }
        public DateTime? LatestArrival { get; set; }
        public int? QuantityOfPassengers { get; set; }
    }
}
