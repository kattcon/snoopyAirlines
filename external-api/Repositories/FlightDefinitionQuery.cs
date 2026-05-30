using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Repositories
{
    public class FlightDefinitionQuery
    {
        public string? Destination { get; set; }
        public IReadOnlyCollection<FlightDefinitionArrivalWindow> ArrivalWindows { get; set; } = [];
        public int? QuantityOfPassengers { get; set; }
    }

    public class FlightDefinitionArrivalWindow
    {
        public FlightFrequency SameDayDepartureFrequency { get; set; } = new();
        public FlightFrequency PreviousDayDepartureFrequency { get; set; } = new();
        public TimeOnly EarliestArrival { get; set; }
        public TimeOnly LatestArrival { get; set; }
    }
}
