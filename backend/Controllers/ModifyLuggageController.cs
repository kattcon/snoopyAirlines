using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snoopy_airlines_backend.Domain.Intake;
using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Services;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;
using System.Linq.Expressions;

namespace snoopy_airlines_backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("modify-luggage")]
    public class ModifyLuggageController : ControllerBase
    {
        private readonly IModifyLuggageService _ModifyLuggageService;
        public ModifyLuggageController(IModifyLuggageService modifyluggageService)
        {
            _ModifyLuggageService = modifyluggageService;
        }

        [AllowAnonymous]
        [HttpGet("getLuggageInfo")]
        public async Task<ActionResult<ModifyLuggageInfo>> searchLuggageInfo(
            [FromQuery] string confirmationNumber,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(confirmationNumber))
                    return BadRequest(new { message = "El número de confirmación es obligatorio." });

                var modifyLuggageInfo = await _ModifyLuggageService.GetLuggageInfoAsync(confirmationNumber, cancellationToken);
                return Ok(modifyLuggageInfo);
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
        [HttpPost("newLuggageInfo")]
        public async Task<IActionResult> modifyLuggage ([FromBody] ModifyLuggageRequest modifyLuggageRequest, CancellationToken cancellationToken)
        {
            Console.WriteLine($"ConfirmationNumber: {modifyLuggageRequest.ConfirmationNumber}");
            Console.WriteLine($"Passengers count: {modifyLuggageRequest.Passengers?.Count}");

            if (string.IsNullOrWhiteSpace(modifyLuggageRequest.ConfirmationNumber))
                return BadRequest(new { message = "El número de confirmación es obligatorio." });
            try
            {
                await _ModifyLuggageService.UpdateLuggageAsync(modifyLuggageRequest, cancellationToken);
                return NoContent();
            }
            catch(ArgumentException ex) 
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
