using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Services
{
    public interface IFlightService
    {
        Task<IReadOnlyCollection<Flight>> GetFlightsAsync(CancellationToken cancellationToken);
    }
}
