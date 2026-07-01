using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Repositories;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;

        public RouteService(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public Task<IReadOnlyCollection<DomainRoute>> GetRoutesAsync(CancellationToken cancellationToken)
        {
            return _routeRepository.GetAllAsync(cancellationToken);
        }

        public Task<DomainRoute?> GetRouteByIdAsync(int routeId, CancellationToken cancellationToken)
        {
            return _routeRepository.GetByIdAsync(routeId, cancellationToken);
        }

        public Task<IReadOnlyCollection<DomainRoute>> SearchRoutesAsync(
            int? departureAirportId,
            int? arrivalAirportId,
            DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            return _routeRepository.SearchAsync(departureAirportId, arrivalAirportId, departureDate, cancellationToken);
        }

        public Task<IReadOnlyCollection<DomainRoute>> SearchRoutesAsync(
            RouteSearchQuery routeQuery,
            CancellationToken cancellationToken)
        {
            return _routeRepository.GetRoutesAsync(routeQuery, cancellationToken);
        }

        public Task SaveRouteAsync(DomainRoute route, CancellationToken cancellationToken)
        {
            return _routeRepository.SaveAsync(route, cancellationToken);
        }

        public async Task<IEnumerable<RouteListItem>> GetRouteListAsync(CancellationToken cancellationToken)
        {

            try
            {
                var routes = await _routeRepository.GetAllWithDetailsAsync(cancellationToken);

                return routes ?? Enumerable.Empty<RouteListItem>();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("No fue posible obtener la lista de rutas.", ex);
            }
        }

        public async Task<bool> DeleteRouteAsync(int routeId, CancellationToken cancellationToken) 
        {
            if (routeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(routeId));

            try
            {
                return await _routeRepository.DeleteAsync(routeId, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Ocurrió un error al eliminar la ruta.", ex);
            }
        }

    }
}
