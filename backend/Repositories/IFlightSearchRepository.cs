using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightSearchRepository
    {
        Task<IReadOnlyCollection<FlightCustomerReport>> GetFlightReportByConfirmationAsync(string confirmationNumber, string lastNames, CancellationToken cancellationToken);
    }
}