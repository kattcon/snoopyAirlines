using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Services;
using System.Text.RegularExpressions;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("airport")]
    public class AirportController : ControllerBase
    {
        private readonly AirportService _airportService;

        public AirportController(AirportService airportService)
        {
            _airportService = airportService;
        }

        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries(CancellationToken cancellationToken)
        {
            var countries = await _airportService.GetCountriesAsync(cancellationToken);

            return Ok(countries);
        }

        [HttpGet("cities")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities(
            [FromQuery] int countryId,
            CancellationToken cancellationToken)
        {
            if (countryId <= 0)
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid query parameters.",
                    Errors = [new ValidationError { Field = nameof(countryId), Message = "countryId must be greater than 0." }]
                });
            }

            var cities = await _airportService.GetCitiesByCountryAsync(countryId, cancellationToken);

            return Ok(cities);
        }

        [HttpPost]
        public async Task<ActionResult<Airport>> Post(
            AirportIntake airportIntake,
            CancellationToken cancellationToken)
        {
            if (!TryMapToAirport(airportIntake, out var airport, out var errors))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Invalid airport payload.",
                    Errors = errors
                });
            }

            try
            {
                var savedAirport = await _airportService.CreateAirportAsync(airport, cancellationToken);

                return Created($"/airport/{savedAirport.Id}", savedAirport);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        private static bool TryMapToAirport(
            AirportIntake airportIntake,
            out Airport airport,
            out IReadOnlyCollection<ValidationError> errors)
        {
            airport = null!;
            var validationErrors = new List<ValidationError>();

            ValidateRequired(nameof(airportIntake.Name), airportIntake.Name, validationErrors);
            ValidateRequired(nameof(airportIntake.Code), airportIntake.Code, validationErrors);
            ValidateRequired(nameof(airportIntake.Timezone), airportIntake.Timezone, validationErrors);

            if (!string.IsNullOrWhiteSpace(airportIntake.Code) && !Regex.IsMatch(airportIntake.Code, @"^[A-Za-z]{3}$"))
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(airportIntake.Code),
                    Message = "Code must be exactly 3 letters (A-Z)."
                });
            }

            if (airportIntake.CityId <= 0)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(airportIntake.CityId),
                    Message = "CityId must be greater than 0."
                });
            }

            if (validationErrors.Count > 0)
            {
                errors = validationErrors;
                return false;
            }

            airport = new Airport
            {
                Name = airportIntake.Name.Trim(),
                Code = airportIntake.Code.Trim().ToUpperInvariant(),
                CityId = airportIntake.CityId,
                Timezone = airportIntake.Timezone.Trim()
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
