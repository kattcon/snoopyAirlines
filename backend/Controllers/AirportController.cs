using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Services;
using System.Text.RegularExpressions;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("airport")]
    public class AirportController : ControllerBase
    {
        // Servicio que contiene la lógica de negocio para aeropuertos
        private readonly AirportService _airportService;

        // El constructor recibe el AirportService mediante inyección de dependencias
        public AirportController(AirportService airportService)
        {
            _airportService = airportService;
        }

        // GET /airport o GET /airport?search=SJO
        // Devuelve la lista de aeropuertos, con filtro opcional por nombre, código o ciudad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirportView>>> GetAirports(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var airports = await _airportService.GetAirportsAsync(search, cancellationToken);

            return Ok(airports);
        }

        // GET /airport/countries
        // Devuelve la lista de todos los países para el formulario de registro
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries(CancellationToken cancellationToken)
        {
            var countries = await _airportService.GetCountriesAsync(cancellationToken);

            return Ok(countries);
        }

        // GET /airport/cities?countryId=5
        // Devuelve las ciudades del país indicado para el formulario de registro
        [HttpGet("cities")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities(
            [FromQuery] int countryId,
            CancellationToken cancellationToken)
        {
            // Valida que el countryId sea un número válido antes de consultar la BD
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

        // POST /airport
        // Registra un nuevo aeropuerto. Valida los datos antes de guardar.
        // Solo los administradores pueden crear aeropuertos  
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Airport>> Post(
            AirportIntake airportIntake,
            CancellationToken cancellationToken)
        {
            // Valida y convierte el intake en un Airport. Si falla, devuelve 400 con los errores
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
                // Llama al servicio para guardar el aeropuerto en la BD
                var savedAirport = await _airportService.CreateAirportAsync(airport, cancellationToken);

                // Devuelve 201 Created con la URL del nuevo recurso y el aeropuerto creado
                return Created($"/airport/{savedAirport.Id}", savedAirport);
            }
            catch (InvalidOperationException ex)
            {
                // Si el código IATA ya existe, devuelve 409 Conflict con el mensaje de error
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpGet("{airportId}")]
        public async Task<ActionResult<Airport>> GetAirportById(int airportId, CancellationToken cancellationToken)
        {
            var airport = await _airportService.GetAirportByIdAsync(airportId, cancellationToken);
            return airport is null ? NotFound() : Ok(airport);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{airportId}")]
        public async Task<IActionResult> UpdateAirportName(
            int airportId,
            AirportNameUpdateIntake airportNameUpdateIntake,
            CancellationToken cancellationToken)
        {
            try
            {
                await _airportService.UpdateAirportNameAsync(airportId, airportNameUpdateIntake.Name, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException invalidNameException)
            {
                return BadRequest(new { Message = invalidNameException.Message });
            }
            catch (KeyNotFoundException airportNotFoundException)
            {
                return NotFound(new { Message = airportNotFoundException.Message });
            }
        }

        private static bool TryMapToAirport(
            AirportIntake airportIntake,
            out Airport airport,
            out IReadOnlyCollection<ValidationError> errors)
        {
            // Empieza sin aeropuerto construido y con la lista de errores vacía
            airport = null!;
            var validationErrors = new List<ValidationError>();

            // Verifica que los campos de texto obligatorios no estén vacíos
            ValidateRequired(nameof(airportIntake.Name), airportIntake.Name, validationErrors);
            ValidateRequired(nameof(airportIntake.Code), airportIntake.Code, validationErrors);

            // Valida que el código IATA tenga exactamente 3 letras (ej: SJO, LAX)
            // Solo lo valida si el código no está vacío para no duplicar el error de "requerido"
            if (!string.IsNullOrWhiteSpace(airportIntake.Code) && !Regex.IsMatch(airportIntake.Code, @"^[A-Za-z]{3}$"))
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(airportIntake.Code),
                    Message = "Code must be exactly 3 letters (A-Z)."
                });
            }

            // Verifica que el CityId sea un número válido (mayor a 0)
            if (airportIntake.CityId <= 0)
            {
                validationErrors.Add(new ValidationError
                {
                    Field = nameof(airportIntake.CityId),
                    Message = "CityId must be greater than 0."
                });
            }

            // Si hubo algún error, retorna false con la lista de errores
            if (validationErrors.Count > 0)
            {
                errors = validationErrors;
                return false;
            }

            // Si todo está bien, construye el Airport con los datos limpios
            // Trim() quita espacios al inicio y al final
            // ToUpperInvariant() convierte el código a mayúsculas (sjo -> SJO)
            airport = new Airport
            {
                Name = airportIntake.Name.Trim(),
                Code = airportIntake.Code.Trim().ToUpperInvariant(),
                CityId = airportIntake.CityId
            };

            errors = Array.Empty<ValidationError>();
            return true;
        }

        // Agrega un error a la lista si el campo está vacío o solo tiene espacios
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

        // Representa un error de validación con el nombre del campo y el mensaje
        private class ValidationError
        {
            required public string Field { get; set; }
            required public string Message { get; set; }
        }

        // Respuesta de error que agrupa todos los errores de validación encontrados
        private class ValidationErrorResponse
        {
            required public string Message { get; set; }
            required public IReadOnlyCollection<ValidationError> Errors { get; set; }
        }
    }
}
