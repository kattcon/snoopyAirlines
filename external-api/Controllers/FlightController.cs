using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.View;
using SnoopyAirlines.External.Services;

namespace SnoopyAirlines.External.Controllers
{
    [ApiController]
    [Route("api/external")]
    public class FlightController : ControllerBase
    {
        private static readonly string[] SupportedDateTimeFormats =
        [
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH-mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "O"
        ];

        private readonly FlightService _flightService;

        public FlightController(FlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        public async Task<ActionResult<FlightsResponse>> Get(
            [FromQuery] string? origin,
            [FromQuery(Name = "detination")] string? detination,
            [FromQuery] string? earliestDeparture,
            [FromQuery] string? latestDeparture,
            [FromQuery] string? quantityOfPassengers,
            [FromQuery] string? apiKey,
            CancellationToken cancellationToken
        ){
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Unauthorized();
            }

            if (!TryCreateFlightQuery(
                origin,
                detination,
                earliestDeparture,
                latestDeparture,
                quantityOfPassengers,
                out var flightQuery,
                out var errors))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid query parameters.",
                    Errors = errors
                });
            }

            var response = await _flightService.Get(flightQuery, cancellationToken);

            return Ok(response);
        }

        private static bool TryCreateFlightQuery(
            string? origin,
            string? detination,
            string? earliestDeparture,
            string? latestDeparture,
            string? quantityOfPassengers,
            out FlightQuery flightQuery,
            out IReadOnlyCollection<ValidationError> errors)
        {
            flightQuery = null!;
            var validationErrors = new List<ValidationError>();

            ValidateAirportCode(nameof(origin), origin, validationErrors);
            ValidateAirportCode("detination", detination, validationErrors);

            var hasEarliestDeparture = TryParseRequiredDateTime(
                nameof(earliestDeparture),
                earliestDeparture,
                validationErrors,
                out var parsedEarliestDeparture);

            var hasLatestDeparture = TryParseRequiredDateTime(
                nameof(latestDeparture),
                latestDeparture,
                validationErrors,
                out var parsedLatestDeparture);

            if (   hasEarliestDeparture
                && hasLatestDeparture
                && parsedLatestDeparture < parsedEarliestDeparture
            ){
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(latestDeparture),
                    Message = "latestDeparture must be greater than or equal to earliestDeparture."
                });
            }

            var parsedQuantityOfPassengers = 1;
            if (   !string.IsNullOrWhiteSpace(quantityOfPassengers)
                && (   !int.TryParse(quantityOfPassengers, out parsedQuantityOfPassengers)
                    || parsedQuantityOfPassengers < 1
                    )
            ){
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(quantityOfPassengers),
                    Message = "quantityOfPassengers must be greater than or equal to 1."
                });
            }

            if (validationErrors.Count > 0)
            {
                errors = validationErrors;
                return false;
            }

            flightQuery = new FlightQuery
            {
                Origin = origin!.Trim().ToUpperInvariant(),
                Destination = detination!.Trim().ToUpperInvariant(),
                EarliestDeparture = parsedEarliestDeparture,
                LatestDeparture = parsedLatestDeparture,
                QuantityOfPassengers = parsedQuantityOfPassengers
            };

            errors = Array.Empty<ValidationError>();
            return true;
        }

        private static void ValidateAirportCode(
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
                return;
            }

            if (!Regex.IsMatch(value.Trim(), @"^[A-Za-z]{3}$"))
            {
                errors.Add(new ValidationError
                {
                    Field = fieldName,
                    Message = $"{fieldName} must be exactly 3 letters (A-Z)."
                });
            }
        }

        private static bool TryParseRequiredDateTime(
            string fieldName,
            string? value,
            ICollection<ValidationError> errors,
            out DateTime parsedDateTime)
        {
            parsedDateTime = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(new ValidationError
                {
                    Field = fieldName,
                    Message = $"{fieldName} is required."
                });
                return false;
            }

            if (DateTime.TryParseExact(
                    value.Trim(),
                    SupportedDateTimeFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out parsedDateTime)
                || DateTime.TryParse(
                    value.Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                    out parsedDateTime))
            {
                return true;
            }

            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{fieldName} must be a valid ISO date time."
            });
            return false;
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
