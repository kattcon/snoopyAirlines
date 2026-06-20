using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain;
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

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyCollection<FlightCustomerReport>>> Search(FlightSearchIntake intake, CancellationToken cancellationToken)
        {
            var flightReport = await _flightSearchService.GetFlightReportByConfirmationAsync(intake.ConfirmationNumber, intake.LastNames, cancellationToken);
            return Ok(flightReport);
        }
    }
}