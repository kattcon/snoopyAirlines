using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Services.PartnerAirlines
{
    public interface IExternalFlightSearchService
    {
        Task<IReadOnlyCollection<FlightResponse>> SearchConnectionsAsync(
            FlightQuery flightQuery,
            CancellationToken cancellationToken);
    }
}
