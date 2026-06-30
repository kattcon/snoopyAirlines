using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Domain;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Repositories
{
    public interface IRouteRepository
    {
        Task<IReadOnlyCollection<DomainRoute>> GetAllAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<DomainRoute>> GetRoutesAsync(RouteSearchQuery routeQuery, CancellationToken cancellationToken = default);
        Task<DomainRoute?> GetByIdAsync(int routeId, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<DomainRoute>> SearchAsync(int? departureAirportId, int? arrivalAirportId, DateOnly? departureDate, CancellationToken cancellationToken);
        Task SaveAsync(DomainRoute route, CancellationToken cancellationToken);

        Task<IEnumerable<RouteListItem>> GetAllWithDetailsAsync(CancellationToken cancellationToken);
    }
}
