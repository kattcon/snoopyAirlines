using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;
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

        [HttpGet("{routeId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<DomainRoute>> GetById(int routeId, CancellationToken cancellationToken)
        {
            var route = await _routeService.GetRouteByIdAsync(routeId, cancellationToken);
            return route is null ? NotFound() : Ok(route);
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

            try
            {
                await _routeService.SaveRouteAsync(route, cancellationToken);
                return CreatedAtAction(nameof(Get), null);

            }
            catch (SqlException exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }

        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RouteListItem>>> GetList(CancellationToken cancellationToken)
        {
            try
            {
                var routes = await _routeService.GetRouteListAsync(cancellationToken);

                return Ok(routes);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocurrió un error al obtener la lista de rutas.");
            }
        }

        [HttpDelete("{routeId:int}")]
        public async Task<IActionResult> Delete(int routeId, CancellationToken cancellationToken)
        {
            if (routeId < 0)
            {
                return BadRequest("El id de la ruta no puede ser menor que cero.");
            }

            try
            {
                var deleted = await _routeService.DeleteRouteAsync(routeId, cancellationToken);

                if (!deleted)
                {
                    return NotFound("La ruta no existe.");
                }

                return NoContent();
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocurrió un error al eliminar la ruta.");
            }
        }
    }
}
