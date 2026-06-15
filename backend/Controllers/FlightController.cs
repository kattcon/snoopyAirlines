using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

        private readonly RouteService _routeService;

        public FlightController(RouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<FlightsResponse>> Search(
            [FromQuery] string? origin,
            [FromQuery(Name = "detination")] string? detination,
            [FromQuery] string? earliestDeparture,
            [FromQuery] string? latestDeparture,
            [FromQuery] string? earliestArrival,
            [FromQuery] string? latestArrival,
            [FromQuery] string? quantityOfPassengers,
            [FromQuery] string? includeStopovers,
            [FromQuery] string? apiKey,
            CancellationToken cancellationToken)
        {
            if (!TryCreateFlightQuery(
                origin,
                detination,
                earliestDeparture,
                latestDeparture,
                earliestArrival,
                latestArrival,
                quantityOfPassengers,
                includeStopovers,
                out var routeQuery,
                out var errors))
            {
                return BadRequest(new
                {
                    Message = "Invalid query parameters.",
                    Errors = errors
                });
            }

            var flights = await _routeService.SearchFlightsAsync(routeQuery, cancellationToken);

            return Ok(new FlightsResponse
            {
                Flights = flights
            });
        }

        private static bool TryCreateFlightQuery(
            string? origin,
            string? detination,
            string? earliestDeparture,
            string? latestDeparture,
            string? earliestArrival,
            string? latestArrival,
            string? quantityOfPassengers,
            string? includeStopovers,
            out RouteQuery routeQuery,
            out IReadOnlyCollection<ValidationError> errors)
        {
            routeQuery = null!;
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

            if (hasEarliestDeparture && hasLatestDeparture && parsedLatestDeparture < parsedEarliestDeparture)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(latestDeparture),
                    Message = "latestDeparture must be greater than or equal to earliestDeparture."
                });
            }

            TryParseOptionalDateTime(
                nameof(earliestArrival),
                earliestArrival,
                validationErrors,
                out var parsedEarliestArrival);

            TryParseOptionalDateTime(
                nameof(latestArrival),
                latestArrival,
                validationErrors,
                out var parsedLatestArrival);

            if (parsedEarliestArrival.HasValue
                && parsedLatestArrival.HasValue
                && parsedLatestArrival.Value < parsedEarliestArrival.Value)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(latestArrival),
                    Message = "latestArrival must be greater than or equal to earliestArrival."
                });
            }

            var parsedQuantityOfPassengers = 1;
            if (!string.IsNullOrWhiteSpace(quantityOfPassengers)
                && (!int.TryParse(quantityOfPassengers, out parsedQuantityOfPassengers)
                    || parsedQuantityOfPassengers < 1))
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(quantityOfPassengers),
                    Message = "quantityOfPassengers must be greater than or equal to 1."
                });
            }

            var parsedIncludeStopovers = false;
            if (!string.IsNullOrWhiteSpace(includeStopovers)
                && !bool.TryParse(includeStopovers, out parsedIncludeStopovers))
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(includeStopovers),
                    Message = "includeStopovers must be a boolean value."
                });
            }

            if (validationErrors.Count > 0)
            {
                errors = validationErrors;
                return false;
            }

            routeQuery = new RouteQuery
            {
                Origin = origin?.Trim().ToUpperInvariant(),
                Destination = detination?.Trim().ToUpperInvariant(),
                EarliestDeparture = parsedEarliestDeparture,
                LatestDeparture = parsedLatestDeparture,
                EarliestArrival = parsedEarliestArrival,
                LatestArrival = parsedLatestArrival,
                QuantityOfPassengers = parsedQuantityOfPassengers,
                IncludeStopovers = parsedIncludeStopovers
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

            if (!Regex.IsMatch(value.Trim(), "^[A-Za-z]{3}$"))
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

            if (TryParseDateTime(value, out parsedDateTime))
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

        private static void TryParseOptionalDateTime(
            string fieldName,
            string? value,
            ICollection<ValidationError> errors,
            out DateTime? parsedDateTime)
        {
            parsedDateTime = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (TryParseDateTime(value, out var dateTime))
            {
                parsedDateTime = dateTime;
                return;
            }

            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{fieldName} must be a valid ISO date time."
            });
        }

        private static bool TryParseDateTime(string value, out DateTime parsedDateTime)
        {
            return DateTime.TryParseExact(
                    value.Trim(),
                    SupportedDateTimeFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out parsedDateTime)
                || DateTime.TryParse(
                    value.Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                    out parsedDateTime);
        }

        private class ValidationError
        {
            required public string Field { get; set; }
            required public string Message { get; set; }
        }
    }
}
