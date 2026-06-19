using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class RouteSearchQuery
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public IReadOnlyCollection<RouteDepartureWindow> DepartureWindows { get; set; } = Array.Empty<RouteDepartureWindow>();
        public IReadOnlyCollection<RouteArrivalWindow> ArrivalWindows { get; set; } = Array.Empty<RouteArrivalWindow>();
        public int? QuantityOfPassengers { get; set; }
        public bool IncludeStopovers { get; set; }
    }

    public class RouteDepartureWindow
    {
        public RouteFrequency Frequency { get; set; } = new();
        public TimeOnly EarliestDeparture { get; set; }
        public TimeOnly LatestDeparture { get; set; }
    }

    public class RouteArrivalWindow
    {
        public RouteFrequency SameDayDepartureFrequency { get; set; } = new();
        public RouteFrequency PreviousDayDepartureFrequency { get; set; } = new();
        public TimeOnly EarliestArrival { get; set; }
        public TimeOnly LatestArrival { get; set; }
    }
}
