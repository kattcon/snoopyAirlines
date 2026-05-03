using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.domain;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;
using System.Text.RegularExpressions;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("airplane")]
    public class AirplaneController : ControllerBase {
        private readonly AirplaneService _airplaneService;
        public AirplaneController(AirplaneService airplaneService)
        {
            _airplaneService = airplaneService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Airplane>>> GetAirplanes(CancellationToken cancellationToken)
        {
            var airplanes = await _airplaneService.GetAirplanesAsync(cancellationToken);
            return Ok(airplanes);
        }


        [HttpPost]
        public async Task<ActionResult<Airplane>> Post(AirplaneIntake airplaneIntake, CancellationToken cancellationToken)
        {
            var airplane = new Airplane
            {
                ModelNumber = airplaneIntake.ModelNumber,
                TouristColumns = airplaneIntake.TouristColumns,
                TouristRows = airplaneIntake.TouristRows,
                FirstclassColumns = airplaneIntake.FirstclassColumns,
                FirstclassRows = airplaneIntake.FirstclassRows,
                MaxWeight = airplaneIntake.MaxWeight
            };

            var savedAirplane = await _airplaneService.CreateAirplaneAsync(airplane, cancellationToken);

            return CreatedAtAction(nameof(Post), savedAirplane);
        }
    }
}
