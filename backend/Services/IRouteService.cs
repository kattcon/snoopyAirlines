using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Repositories;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services
{
    public interface IRouteService
    {
        Task<IReadOnlyCollection<DomainRoute>> GetRoutesAsync(CancellationToken cancellationToken);

        Task<DomainRoute?> GetRouteByIdAsync(int routeId, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<DomainRoute>> SearchRoutesAsync(
            int? departureAirportId,
            int? arrivalAirportId,
            DateOnly? departureDate,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<DomainRoute>> SearchRoutesAsync(
            RouteSearchQuery routeQuery,
            CancellationToken cancellationToken);

        Task SaveRouteAsync(DomainRoute route, CancellationToken cancellationToken);

        Task<IEnumerable<RouteListItem>> GetRouteListAsync(CancellationToken cancellationToken);
    }
}
