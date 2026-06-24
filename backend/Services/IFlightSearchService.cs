using SnoopyAirlines.Domain.View;
using snoopy_airlines_backend.Domain.View;

namespace SnoopyAirlines.Services
{
    public interface IFlightSearchService
    {
        Task<FlightSearchResult> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );
    }
}