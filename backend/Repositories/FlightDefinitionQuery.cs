using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class FlightDefinitionQuery
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public IReadOnlyCollection<FlightDefinitionDepartureWindow> DepartureWindows { get; set; } = Array.Empty<FlightDefinitionDepartureWindow>();
        public int? QuantityOfPassengers { get; set; }
        public bool IncludeStopovers { get; set; }
    }

    public class FlightDefinitionDepartureWindow
    {
        public FlightFrequency Frequency { get; set; } = new();
        public TimeOnly EarliestDeparture { get; set; }
        public TimeOnly LatestDeparture { get; set; }
    }
}
