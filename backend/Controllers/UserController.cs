using Microsoft.AspNetCore.Authorization;
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
        private readonly TokenService _tokenService;

        public UserController(
            UserService userService,
            TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }
        // Solo los administradores pueden listar los usuarios registrados y pendientes de registro
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserView>>> Get(CancellationToken cancellationToken)
        {
            var users = await _userService.GetUsersAsync(cancellationToken);

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<UserView>> Post(
            UserIntake userIntake,
            CancellationToken cancellationToken)
        {
            // Handle invalid payload
            if (!TryMapToPendingUser(userIntake, out var pendingUser, out var errors))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid user payload.",
                    Errors = errors
                });
            }

            // Handle existing full registered user
            if (await _userService.ExistsByEmailAsync(pendingUser.Email, cancellationToken))
            {
                var savedUser = await _userService.SaveUserAsync(ToUser(pendingUser), cancellationToken);

                return Ok(savedUser);
            }

            // New pending user
            var registrationKey = _userService.GenerateRegistrationKey();
            pendingUser.RegistrationKeyHash = registrationKey.Hash;

            bool pendingUserExists = await _userService.ExistsPendingByEmailAsync(pendingUser.Email, cancellationToken);
            var savedPendingUser = await _userService.SavePendingUserAsync(pendingUser, cancellationToken);
            var savedPendingUserView = ToUserView(savedPendingUser);

            await _userService.SendRegistrationEmailAsync(
                savedPendingUser.Email,
                registrationKey.Value,
                cancellationToken);

            if (pendingUserExists)
            {
                return Ok(savedPendingUserView);
            }

            return Created($"/user/{savedPendingUserView.Id}", savedPendingUserView);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [HttpPost("registration")]
        public async Task<ActionResult<UserView>> Registration(
            UserRegistrationIntake registrationIntake,
            CancellationToken cancellationToken)
        {
            var validationErrors = new List<ValidationError>();

            ValidateRequired(nameof(registrationIntake.Password), registrationIntake.Password, validationErrors);
            ValidateRequired(nameof(registrationIntake.PendingKey), registrationIntake.PendingKey, validationErrors);

            if (!string.IsNullOrWhiteSpace(registrationIntake.Password))
            {
                AddPasswordValidationErrors(registrationIntake.Password, validationErrors);
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid registration payload.",
                    Errors = validationErrors
                });
            }

            try
            {
                var savedUser = await _userService.RegisterPendingUserAsync(
                    registrationIntake.PendingKey!.Trim(),
                    registrationIntake.Password!,
                    cancellationToken);

                if (savedUser is null)
                {
                    return BadRequest(new { Message = "Invalid registration key." });
                }

                return Ok(savedUser);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { ex.Message });
            }
            catch (ArgumentException ex) when (ex.ParamName == "password")
            {
                AddPasswordValidationErrors(registrationIntake.Password, validationErrors);

                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid registration payload.",
                    Errors = validationErrors
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(
            LoginIntake loginIntake,
            CancellationToken cancellationToken)
        {
            var validationErrors = new List<ValidationError>();

            ValidateRequired(nameof(loginIntake.Email), loginIntake.Email, validationErrors);
            ValidateRequired(nameof(loginIntake.Password), loginIntake.Password, validationErrors);

            if (validationErrors.Count > 0)
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid login payload.",
                    Errors = validationErrors
                });
            }

            var user = await _userService.LoginAsync(
                loginIntake.Email!.Trim(),
                loginIntake.Password!.Trim(),
                cancellationToken
            );

            if (user is null)
            {
                return Unauthorized();
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new {token});
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserView>> GetById(int userId, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}")]
        public async Task<ActionResult<UserView>> AdminUpdate(
            int userId,
            AdminUserUpdateIntake intake,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryMapRole(intake.Type, out _))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid user payload.",
                    Errors = new[]
                    {
                        new ValidationError
                        {
                            Field = nameof(intake.Type),
                            Message = "Type must be 'ad' or 'op'."
                        }
                    }
                });
            }

            var updatedUser = await _userService.AdminUpdateUserAsync(userId, intake, cancellationToken);

            if (updatedUser is null)
            {
                return NotFound();
            }

            return Ok(updatedUser);
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

        private static User ToUser(PendingUser pendingUser)
        {
            return new User
            {
                IdentificationNumber = pendingUser.IdentificationNumber,
                Email = pendingUser.Email,
                FirstName = pendingUser.FirstName,
                LastNameOne = pendingUser.LastNameOne,
                LastNameTwo = pendingUser.LastNameTwo,
                Type = pendingUser.Type,
                PasswordHash = string.Empty,
                PasswordSalt = string.Empty
            };
        }

        private static UserView ToUserView(PendingUser pendingUser)
        {
            return new UserView
            {
                Id = pendingUser.Id.GetValueOrDefault(),
                IdentificationNumber = pendingUser.IdentificationNumber,
                Email = pendingUser.Email,
                FirstName = pendingUser.FirstName,
                LastNameOne = pendingUser.LastNameOne,
                LastNameTwo = pendingUser.LastNameTwo,
                Type = pendingUser.Type,
                Pending = true
            };
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

        private static void AddPasswordValidationErrors(
            string? password,
            ICollection<ValidationError> errors)
        {
            foreach (var message in UserService.ValidatePassword(password))
            {
                errors.Add(new ValidationError
                {
                    Field = nameof(UserRegistrationIntake.Password),
                    Message = message
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
