using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly FlightService _flightService;

        public FlightController(FlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Flight>>> Get(
            [FromQuery] int? departureAirportId,
            [FromQuery] int? arrivalAirportId,
            [FromQuery] DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            // Si hay parámetros de búsqueda, usa la búsqueda filtrada
            if (departureAirportId.HasValue || arrivalAirportId.HasValue || departureDate.HasValue)
            {
                var flights = await _flightService.SearchFlightsAsync(
                    departureAirportId,
                    arrivalAirportId,
                    departureDate,
                    cancellationToken);

                return Ok(flights);
            }

            // Si no hay filtros, devuelve todos
            var allFlights = await _flightService.GetFlightsAsync(cancellationToken);

            return Ok(allFlights);
        }

        [HttpPost]
        public async Task<ActionResult<Flight>> Post(
            Flight flight,
            CancellationToken cancellationToken)
        {
            if (flight.Frequency is null || !flight.Frequency.HasAnyDay())
            {
                return BadRequest(new { frequency = "At least one day must be selected." });
            }

            var savedFlight = await _flightService.SaveFlightAsync(flight, cancellationToken);

            return CreatedAtAction(nameof(Get), savedFlight);
        }
    }
}
