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
        private readonly IAirplaneService _airplaneService;
        public AirplaneController(IAirplaneService airplaneService)
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

        [HttpGet("id/{airplaneId}")]
        public async Task<ActionResult<Airplane>> GetAirplaneById(int airplaneId, CancellationToken cancellationToken)
        {
            var airplane = await _airplaneService.GetAirplaneByIdAsync(airplaneId, cancellationToken);

            if (airplane == null)
                return NotFound($"No se encontró una aeronave con el id {airplaneId}.");

            return Ok(airplane);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{airplaneId}")]
        public async Task<IActionResult> UpdateAirplaneCapacities(int airplaneId, AirplaneUpdateIntake intake, CancellationToken cancellationToken)
        {
            try
            {
                await _airplaneService.UpdateAirplaneCapacitiesAsync(airplaneId, intake, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{airplaneId}")]
        public async Task<IActionResult> DeleteAirplane(int airplaneId, CancellationToken cancellationToken)
        {
            try
            {
                await _airplaneService.DeleteAirplaneAsync(airplaneId, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
        }
    }
}
