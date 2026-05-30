namespace SnoopyAirlines.Domain.View
{
    public class FlightsResponse
    {
        required public IReadOnlyCollection<FlightResponse> Flights { get; set; }
    }
}
