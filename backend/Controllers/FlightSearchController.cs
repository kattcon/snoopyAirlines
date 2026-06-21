using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("flight-search")]
    public class FlightSearchController : ControllerBase
    {
        private readonly IFlightSearchService _flightSearchService;

        public FlightSearchController(IFlightSearchService flightSearchService)
        {
            _flightSearchService = flightSearchService;
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyCollection<FlightReportView>>> Search(
            [FromQuery] string confirmationNumber,
            [FromQuery] string lastNames,
            CancellationToken cancellationToken)
        {
            try
            {
                var flightReport = await _flightSearchService.GetFlightReportByConfirmationAsync(
                confirmationNumber,
                lastNames,
                cancellationToken);

                return Ok(flightReport);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            
        }
    }
}