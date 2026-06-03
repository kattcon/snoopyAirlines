using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Domain.View
{
    public class FlightsResponse
    {
        required public IReadOnlyCollection<Flight> Flights { get; set; }
    }
}
