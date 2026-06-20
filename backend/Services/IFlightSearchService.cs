using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Services
{
    public interface IFlightSearchService
    {
        Task<IReadOnlyCollection<FlightCustomerReport>> GetFlightReportByConfirmationAsync(string confirmation_code, string last_Names, CancellationToken cancellationToken);
    }
}