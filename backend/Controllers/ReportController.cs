using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("airline-detailed")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<ActionResult<IReadOnlyCollection<AirlineDetailedReportRow>>> GetAirlineDetailed(
            [FromQuery] string? origin,
            [FromQuery] string? destination,
            [FromQuery] string? seatClass,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            CancellationToken cancellationToken)
        {
            var rows = await _reportService.GetAirlineDetailedReportAsync(
                origin,
                destination,
                seatClass,
                dateFrom,
                dateTo,
                cancellationToken);

            return Ok(rows);
        }
    }
}
