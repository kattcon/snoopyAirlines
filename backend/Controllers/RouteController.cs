using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Services;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly RouteService _routeService;

        public RouteController(RouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DomainRoute>>> Get(
            [FromQuery] int? departureAirportId,
            [FromQuery] int? arrivalAirportId,
            [FromQuery] DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            // Si hay parámetros de búsqueda, usa la búsqueda filtrada
            if (departureAirportId.HasValue || arrivalAirportId.HasValue || departureDate.HasValue)
            {
                var routes = await _routeService.SearchRoutesAsync(
                    departureAirportId,
                    arrivalAirportId,
                    departureDate,
                    cancellationToken);

                return Ok(routes);
            }

            // Si no hay filtros, devuelve todos
            var allRoutes = await _routeService.GetRoutesAsync(cancellationToken);

            return Ok(allRoutes);
        }

        [HttpPost]
        public async Task<ActionResult<DomainRoute>> Post(
            DomainRoute route,
            CancellationToken cancellationToken)
        {
            if (route.Frequency is null || !route.Frequency.HasAnyDay())
            {
                return BadRequest(new { frequency = "At least one day must be selected." });
            }

            var savedRoute = await _routeService.SaveRouteAsync(route, cancellationToken);

            return CreatedAtAction(nameof(Get), savedRoute);
        }
    }
}
