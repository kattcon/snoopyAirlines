using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class AirportRepository
    {
        // Cadena de conexión a la base de datos, leída desde appsettings.json
        private readonly string _connectionString;

        // El constructor recibe IConfiguration mediante inyección de dependencias
        // y extrae la cadena de conexión. Si no existe, lanza un error claro.
        public AirportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        // Devuelve todos los países ordenados alfabéticamente.
        // AS Id, AS Name mapean las columnas de la BD a las propiedades del modelo Country.
        public async Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT id AS Id, name AS Name FROM country ORDER BY name;";

            await using var connection = new SqlConnection(_connectionString);
            var countries = await connection.QueryAsync<Country>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return countries.ToList();
        }

        // Devuelve las ciudades que pertenecen a un país específico, ordenadas alfabéticamente.
        // @CountryId es un parámetro seguro que evita SQL injection.
        public async Task<IReadOnlyCollection<City>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT id AS Id, name AS Name, country_id AS CountryId
                FROM city
                WHERE country_id = @CountryId
                ORDER BY name;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var cities = await connection.QueryAsync<City>(
                new CommandDefinition(sql, new { CountryId = countryId }, cancellationToken: cancellationToken));

            return cities.ToList();
        }

        // Verifica si ya existe un aeropuerto con el código IATA dado.
        // COUNT(1) devuelve 0 si no existe, o un número mayor si existe.
        public async Task<bool> AirportCodeExistsAsync(string code, CancellationToken cancellationToken)
        {
            const string sql = "SELECT COUNT(1) FROM airport WHERE code = @Code;";

            await using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));

            return count > 0;
        }

        // Inserta un nuevo aeropuerto y devuelve el registro completo tal como quedó en la BD,
        // incluyendo el Id generado automáticamente por IDENTITY.
        // OUTPUT INSERTED permite obtener la fila insertada en una sola operación.
        public async Task<Airport> CreateAirportAsync(Airport airport, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO airport (name, code, city_id)
                OUTPUT
                    INSERTED.id AS Id,
                    INSERTED.name AS Name,
                    INSERTED.code AS Code,
                    INSERTED.city_id AS CityId
                VALUES (@Name, @Code, @CityId);
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<Airport>(
                new CommandDefinition(sql, airport, cancellationToken: cancellationToken));
        }

        // Devuelve todos los aeropuertos con nombre de ciudad y país resueltos mediante JOINs.
        // Si se proporciona un término de búsqueda, filtra por nombre del aeropuerto, código o ciudad.
        public async Task<IReadOnlyCollection<AirportView>> GetAirportsAsync(string? search, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    a.id AS Id,
                    a.name AS Name,
                    a.code AS Code,
                    ci.name AS CityName,
                    co.name AS CountryName
                FROM airport a
                INNER JOIN city ci ON a.city_id = ci.id
                INNER JOIN country co ON ci.country_id = co.id
                WHERE @Search IS NULL
                   OR a.name LIKE '%' + @Search + '%'
                   OR a.code LIKE '%' + @Search + '%'
                   OR ci.name LIKE '%' + @Search + '%'
                ORDER BY a.name;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var airports = await connection.QueryAsync<AirportView>(
                new CommandDefinition(sql, new { Search = search }, cancellationToken: cancellationToken));

            return airports.ToList();
        }

        public async Task<Airport?> GetAirportByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT id AS Id, name AS Name, code AS Code, city_id AS CityId
                FROM airport
                WHERE id = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Airport>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        }

        public async Task<bool> UpdateAirportNameAsync(int id, string name, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE airport SET name = @Name WHERE id = @Id;";

            await using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { Id = id, Name = name }, cancellationToken: cancellationToken));

            return rowsAffected > 0;
        }
    }
}