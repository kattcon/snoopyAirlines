using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Services;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;

namespace snoopy_airlines_backend.Controllers
{
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


    }
}
