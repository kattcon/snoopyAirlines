using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Services
{
    public interface IFlightService
    {
        Task<IReadOnlyCollection<FlightResponse>> Search(
            FlightQuery flightQuery,
            CancellationToken cancellationToken);

        Task<FlightLegResponse?> GetByGuidAsync(
            Guid flightGuid,
            CancellationToken cancellationToken);

        Task<FlightSearchResult> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);
    }
}
