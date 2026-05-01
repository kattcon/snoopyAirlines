using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.Intake;
using SnoopyAirlines.External.Services;

namespace SnoopyAirlines.External.Controllers
{
    [ApiController]
    [Route("api/external/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiUser>> Post(
            [FromQuery] string? apiKey,
            [FromBody] ApiUserIntake apiUserIntake,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Unauthorized();
            }

            var requester = await _userService.GetByApiKey(apiKey.Trim(), cancellationToken);

            if (requester is null)
            {
                return Unauthorized();
            }

            if (requester.Role != UserService.AdminRole)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (!TryMapToApiUser(apiUserIntake, out var apiUser, out var errors))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid user payload.",
                    Errors = errors
                });
            }

            var savedUser = await _userService.Create(
                apiUser.Name,
                apiUser.Role,
                cancellationToken);

            return Created($"/api/external/user/{savedUser.Id}", savedUser);
        }

        private static bool TryMapToApiUser(
            ApiUserIntake apiUserIntake,
            out ApiUser apiUser,
            out IReadOnlyCollection<ValidationError> errors)
        {
            apiUser = null!;
            var validationErrors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(apiUserIntake.Name))
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(apiUserIntake.Name),
                    Message = "Name is required."
                });
            }
            else if (apiUserIntake.Name.Trim().Length > 32)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(apiUserIntake.Name),
                    Message = "Name must be 32 characters or fewer."
                });
            }

            var role = string.IsNullOrWhiteSpace(apiUserIntake.Role)
                ? UserService.UserRole
                : apiUserIntake.Role.Trim().ToUpperInvariant();

            if (role is not UserService.AdminRole and not UserService.UserRole)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(apiUserIntake.Role),
                    Message = "Role must be AD or US."
                });
            }

            if (validationErrors.Count > 0)
            {
                errors = validationErrors;
                return false;
            }

            apiUser = new ApiUser
            {
                ApiKey = string.Empty,
                Name = apiUserIntake.Name!.Trim(),
                Role = role
            };

            errors = Array.Empty<ValidationError>();
            return true;
        }

        private class ValidationError
        {
            required public string Field { get; set; }
            required public string Message { get; set; }
        }

        private class ValidationErrorResponse
        {
            required public string Message { get; set; }
            required public IReadOnlyCollection<ValidationError> Errors { get; set; }
        }
    }
}
