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

        [HttpGet("monthly-revenue")]
        public async Task<ActionResult<MonthlyRevenueReport>> GetMonthlyRevenueAsync(
            [FromQuery] int year,
            CancellationToken cancellationToken)
        {
            if (year < 1 || year > 9999)
            {
                return BadRequest(new { Message = "year must be between 1 and 9999." });
            }

            var report = await _reportService.GetMonthlyRevenueReportAsync(year, cancellationToken);
            return Ok(report);
        }
    }
}