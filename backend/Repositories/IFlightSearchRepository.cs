using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightSearchRepository
    {
        Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);
    }
}