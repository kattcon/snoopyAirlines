using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;
using System.Security.Claims;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("user")]
    public class AdminUserController : ControllerBase
    {
        private readonly UserService _userService;

        public AdminUserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserView>> GetById(int id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                return NotFound(new { Message = "User not found", SearchedId = id });
            }

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserView>> AdminUpdate(
            int id,
            AdminUserUpdateIntake intake,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryMapRole(intake.Type, out _))
            {
                return BadRequest(new
                {
                    Message = "Invalid user payload.",
                    Errors = new[]
                    {
                        new { Field = nameof(intake.Type), Message = "Type must be 'ad' or 'op'." }
                    }
                });
            }

            var updatedUser = await _userService.AdminUpdateUserAsync(id, intake, cancellationToken);

            if (updatedUser is null)
            {
                return NotFound();
            }

            return Ok(updatedUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var actorUserId))
            {
                return Unauthorized();
            }

            var result = await _userService.DeleteUserAsync(actorUserId, id, cancellationToken);

            return result switch
            {
                DeleteUserResult.Success => NoContent(),
                DeleteUserResult.SelfDelete => Conflict(new { Message = "You cannot delete your own user." }),
                DeleteUserResult.ProtectedInitialAdmin => Conflict(new { Message = "The initial admin user cannot be deleted." }),
                DeleteUserResult.NotFound => NotFound(new { Message = "User not found.", SearchedId = id }),
                DeleteUserResult.Forbidden => Forbid(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        private static bool TryMapRole(string? roleValue, out UserRole role)
        {
            switch (roleValue?.Trim().ToLowerInvariant())
            {
                case "ad":
                    role = UserRole.Admin;
                    return true;
                case "op":
                    role = UserRole.Operator;
                    return true;
                default:
                    role = default;
                    return false;
            }
        }

        private bool TryGetAuthenticatedUserId(out int userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claim, out userId))
            {
                return true;
            }

            userId = default;
            return false;
        }
    }
}
