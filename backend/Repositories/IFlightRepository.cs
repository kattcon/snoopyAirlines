using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightRepository
    {
        Task<IReadOnlyCollection<Flight>> GetAllAsync(CancellationToken cancellationToken);
    }
}
