using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Services
{
    public interface IFlightSearchService
    {
        Task<FlightSearchResult> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);
    }
}