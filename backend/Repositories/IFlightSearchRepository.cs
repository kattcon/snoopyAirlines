using snoopy_airlines_backend.Domain.View;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightSearchRepository
    {
        Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );
    }
}