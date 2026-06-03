using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Repositories
{
    public class RouteSearchQuery
    {
        public string? Destination { get; set; }
        public IReadOnlyCollection<RouteArrivalWindow> ArrivalWindows { get; set; } = [];
        public int? QuantityOfPassengers { get; set; }
    }

    public class RouteArrivalWindow
    {
        public RouteFrequency SameDayDepartureFrequency { get; set; } = new();
        public RouteFrequency PreviousDayDepartureFrequency { get; set; } = new();
        public TimeOnly EarliestArrival { get; set; }
        public TimeOnly LatestArrival { get; set; }
    }
}
