using Microsoft.AspNetCore.Authorization;
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
        
        [HttpGet("{model}")]
        public async Task<ActionResult<Airplane>> GetAirplane(string model, CancellationToken cancellationToken)
        {
            var airplane = await _airplaneService.GetAirplaneByModelAync(model, cancellationToken);
            if (airplane == null)
            {
                return NotFound($"Airplane model ' {model}' not found");
            } else
            {
                return Ok(airplane);
            }
        }
        [Authorize(Roles = "Admin")] //  agregar operadores
        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<Airplane>>> GetAirplanes(CancellationToken cancellationToken)
        {
            var airplanes = await _airplaneService.GetAirplanesAsync(cancellationToken);
            return Ok(airplanes);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Airplane>> Post(AirplaneIntake airplaneIntake, CancellationToken cancellationToken)
        {

            var exists = await _airplaneService.ExistsByModelAsync(airplaneIntake.Model, cancellationToken);
            if (exists)
            {
                return Conflict($"Airplane model '{airplaneIntake.Model}' already exists");
            }


            var airplane = new Airplane
            {
                Model = airplaneIntake.Model,
                TouristColumns = airplaneIntake.TouristColumns,
                TouristRows = airplaneIntake.TouristRows,
                FirstclassColumns = airplaneIntake.FirstclassColumns,
                FirstclassRows = airplaneIntake.FirstclassRows,
                MaxWeight = airplaneIntake.MaxWeight
            };

            var savedAirplane = await _airplaneService.CreateAirplaneAsync(airplane, cancellationToken);

            return CreatedAtAction(nameof(GetAirplane),
                new { model = savedAirplane.Model }, savedAirplane );
        }
    }
}
