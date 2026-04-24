using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;
using System.Collections.Generic;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserView>>> Get(CancellationToken cancellationToken)
        {
            var users = await _userService.GetUsersAsync(cancellationToken);

            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<PendingUser>> Post(
            UserIntake userIntake,
            CancellationToken cancellationToken)
        {
            if (!TryMapToPendingUser(userIntake, out var pendingUser, out var errors))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid user payload.",
                    Errors = errors
                });
            }

            var savedPendingUser = await _userService.SavePendingUserAsync(pendingUser, cancellationToken);

            return Created($"/user/{savedPendingUser.Id}", savedPendingUser);
        }

        private static bool TryMapToPendingUser(
            UserIntake userIntake,
            out PendingUser pendingUser,
            out IReadOnlyCollection<ValidationError> errors)
        {
            pendingUser = null!;
            var validationErrors = new List<ValidationError>();

            ValidateRequired(nameof(userIntake.IdentificationNumber), userIntake.IdentificationNumber, validationErrors);
            ValidateRequired(nameof(userIntake.Email), userIntake.Email, validationErrors);
            ValidateRequired(nameof(userIntake.FirstName), userIntake.FirstName, validationErrors);
            ValidateRequired(nameof(userIntake.LastNameOne), userIntake.LastNameOne, validationErrors);
            ValidateRequired(nameof(userIntake.Type), userIntake.Type, validationErrors);

            if (!TryMapRole(userIntake.Type, out var role))
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(userIntake.Type),
                    Message = "Type must be 'ad' or 'op'."
                });
            }

            if (validationErrors.Count > 0)
            {
                errors = validationErrors;
                return false;
            }

            pendingUser = new PendingUser
            {
                Id = userIntake.Id,
                IdentificationNumber = userIntake.IdentificationNumber.Trim(),
                Email = userIntake.Email.Trim(),
                FirstName = userIntake.FirstName.Trim(),
                LastNameOne = userIntake.LastNameOne.Trim(),
                LastNameTwo = string.IsNullOrWhiteSpace(userIntake.LastNameTwo)
                    ? null
                    : userIntake.LastNameTwo.Trim(),
                Type = role
            };

            errors = Array.Empty<ValidationError>();
            return true;
        }

        private static void ValidateRequired(
            string fieldName,
            string? value,
            ICollection<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(new ValidationError
                {
                    Field = fieldName,
                    Message = $"{fieldName} is required."
                });
            }
        }

        private static bool TryMapRole(string roleValue, out UserRole role)
        {
            switch (roleValue.Trim().ToLowerInvariant())
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
