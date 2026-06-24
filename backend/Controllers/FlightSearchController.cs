using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Services;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("flight-search")]
    public class FlightSearchController : ControllerBase
    {
        private readonly IFlightSearchService _flightSearchService;
        private readonly IModifyluggageService _ModifyLuggageService;
        public FlightSearchController(IFlightSearchService flightSearchService, IModifyluggageService modifyluggageService)
        {
            _flightSearchService = flightSearchService;
            _ModifyLuggageService = modifyluggageService;
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

        [AllowAnonymous]
        [HttpGet("searchPassengers")]
        public async Task<ActionResult<IReadOnlyCollection<PassengerView>>> searchPassengers(
            [FromQuery] string confirmationNumber,
            CancellationToken cancellationToken)
        {
            try
            {
                var passengersList = await _ModifyLuggageService.GetPassengersByConfirmationAsync(confirmationNumber, cancellationToken);
                return Ok(passengersList);
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