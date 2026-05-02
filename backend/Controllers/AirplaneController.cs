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

        [HttpPost]
        public /*async*/ Task<ActionResult<Airplane>> Post(AirplaneIntake airplaneIntake, CancellationToken cancellationToken)
        {
            var savedAirplane = 0;

            //return CreatedAtAction(nameof(Get), savedAirplane);

        }



    
    
    }




}
