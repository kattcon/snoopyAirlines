using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Services
{
    public interface IFlightService
    {
        Task<IReadOnlyCollection<FlightResponse>> Search(
            FlightQuery flightQuery,
            CancellationToken cancellationToken);
    }
}
