using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Services
{
    public interface IFlightSearchService
    {
        Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);
    }
}