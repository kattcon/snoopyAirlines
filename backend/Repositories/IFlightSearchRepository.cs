using SnoopyAirlines.Domain.Intake;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightSearchRepository
    {
        Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(string email, string password, CancellationToken cancellationToken);
    }
}