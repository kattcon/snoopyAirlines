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

        private readonly FlightService _flightService;

        public FlightController(FlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Flight>>> Get(
            [FromQuery] int? departureAirportId,
            [FromQuery] int? arrivalAirportId,
            [FromQuery] DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            // Si hay parámetros de búsqueda, usa la búsqueda filtrada
            if (departureAirportId.HasValue || arrivalAirportId.HasValue || departureDate.HasValue)
            {
                var flights = await _flightService.SearchFlightsAsync(
                    departureAirportId,
                    arrivalAirportId,
                    departureDate,
                    cancellationToken);

                return Ok(flights);
            }

            // Si no hay filtros, devuelve todos
            var allFlights = await _flightService.GetFlightsAsync(cancellationToken);

            return Ok(allFlights);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<FlightsResponse>> Search(
            [FromQuery] string? origin,
            [FromQuery(Name = "detination")] string? detination,
            [FromQuery] string? earliestDeparture,
            [FromQuery] string? latestDeparture,
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
                quantityOfPassengers,
                includeStopovers,
                out var flightQuery,
                out var errors))
            {
                return BadRequest(new
                {
                    Message = "Invalid query parameters.",
                    Errors = errors
                });
            }

            var flights = await _flightService.SearchFlightsAsync(flightQuery, cancellationToken);

            return Ok(new FlightsResponse
            {
                Flights = flights
            });
        }

        [HttpPost]
        public async Task<ActionResult<Flight>> Post(
            Flight flight,
            CancellationToken cancellationToken)
        {
            if (flight.Frequency is null || !flight.Frequency.HasAnyDay())
            {
                return BadRequest(new { frequency = "At least one day must be selected." });
            }

            var savedFlight = await _flightService.SaveFlightAsync(flight, cancellationToken);

            return CreatedAtAction(nameof(Get), savedFlight);
        }

        private static bool TryCreateFlightQuery(
            string? origin,
            string? detination,
            string? earliestDeparture,
            string? latestDeparture,
            string? quantityOfPassengers,
            string? includeStopovers,
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

            if (hasEarliestDeparture && hasLatestDeparture && parsedLatestDeparture < parsedEarliestDeparture)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(latestDeparture),
                    Message = "latestDeparture must be greater than or equal to earliestDeparture."
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

            flightQuery = new FlightQuery
            {
                Origin = origin?.Trim().ToUpperInvariant(),
                Destination = detination?.Trim().ToUpperInvariant(),
                EarliestDeparture = parsedEarliestDeparture,
                LatestDeparture = parsedLatestDeparture,
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
    }
}
