using Microsoft.AspNetCore.Mvc;
using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("reports")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("monthly-revenue/filters")]
        public async Task<ActionResult<MonthlyRevenueFilterOptions>> GetMonthlyRevenueFiltersAsync(
            CancellationToken cancellationToken)
        {
            var filters = await _reportService.GetMonthlyRevenueFilterOptionsAsync(cancellationToken);
            return Ok(filters);
        }

        [HttpGet("monthly-revenue")]
        public async Task<ActionResult<MonthlyRevenueReport>> GetMonthlyRevenueAsync(
            [FromQuery] int? year,
            [FromQuery] int? originAirportId,
            [FromQuery] int? destinationAirportId,
            [FromQuery] int? airplaneId,
            CancellationToken cancellationToken)
        {
            if (year.HasValue && (year.Value < 1 || year.Value > 9999))
            {
                return BadRequest(new { Message = "year must be between 1 and 9999." });
            }

            if (originAirportId.HasValue && originAirportId.Value <= 0)
            {
                return BadRequest(new { Message = "originAirportId must be greater than 0." });
            }

            if (destinationAirportId.HasValue && destinationAirportId.Value <= 0)
            {
                return BadRequest(new { Message = "destinationAirportId must be greater than 0." });
            }

            if (airplaneId.HasValue && airplaneId.Value <= 0)
            {
                return BadRequest(new { Message = "airplaneId must be greater than 0." });
            }

            var report = await _reportService.GetMonthlyRevenueReportAsync(
                year,
                originAirportId,
                destinationAirportId,
                airplaneId,
                cancellationToken);
            return Ok(report);
        }
    }
}