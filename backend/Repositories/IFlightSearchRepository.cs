using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightSearchRepository
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