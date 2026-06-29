namespace SnoopyAirlines.Domain.View
{
    public class FlightSearchResult
    {
        public IReadOnlyCollection<FlightReportView> Legs { get; init; } = [];
        public IReadOnlyCollection<PassengerReportView> Passengers { get; init; } = [];
    }
}