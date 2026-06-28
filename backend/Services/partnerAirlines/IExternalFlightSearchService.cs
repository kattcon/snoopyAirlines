using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Services.PartnerAirlines
{
    public interface IExternalFlightSearchService
    {
        Task<IReadOnlyCollection<ExternalFlight>> SearchConnectionsAsync(
            ExternalFlightSearchQuery flightQuery,
            CancellationToken cancellationToken);
    }
}
