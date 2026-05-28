using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.External.Services;

namespace SnoopyAirlines.External.Controllers
{
    [ApiController]
    [Route("api/external")]
    public class AirportController : ControllerBase
    {
        private readonly AirportService _airportService;

        public AirportController(AirportService airportService)
        {
            _airportService = airportService;
        }

        [HttpGet("airports")]
        public async Task<ActionResult<IReadOnlyCollection<AirportDto>>> GetAirports(
            CancellationToken cancellationToken)
        {
            var airports = await _airportService.GetAllAirports(cancellationToken);

            var result = airports.Select(a => new AirportDto
            {
                Code = a.Code,
                Name = a.Name,
                City = a.City
            }).ToList();

            return Ok(result);
        }
    }

    public class AirportDto
    {
        required public string Code { get; set; }
        required public string Name { get; set; }
        required public string City { get; set; }
    }
}
