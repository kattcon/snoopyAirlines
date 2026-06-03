using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class RouteSearchQuery
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public IReadOnlyCollection<RouteDepartureWindow> DepartureWindows { get; set; } = Array.Empty<RouteDepartureWindow>();
        public int? QuantityOfPassengers { get; set; }
        public bool IncludeStopovers { get; set; }
    }

    public class RouteDepartureWindow
    {
        public RouteFrequency Frequency { get; set; } = new();
        public TimeOnly EarliestDeparture { get; set; }
        public TimeOnly LatestDeparture { get; set; }
    }
}
