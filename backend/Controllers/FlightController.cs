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
        public async Task<ActionResult<IEnumerable<Flight>>> Get(CancellationToken cancellationToken)
        {
            var flights = await _flightService.GetFlightsAsync(cancellationToken);

            return Ok(flights);
        }

        [HttpPost]
        public async Task<ActionResult<Flight>> Post(
            Flight flight,
            CancellationToken cancellationToken)
        {
            var savedFlight = await _flightService.SaveFlightAsync(flight, cancellationToken);

            return CreatedAtAction(nameof(Get), savedFlight);
        }
    }
}
