using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class AirportRepository : IAirportRepository
    {
        private readonly string _connectionString;

        public AirportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT id AS Id, name AS Name FROM country ORDER BY name;";

            await using var connection = new SqlConnection(_connectionString);
            var countries = await connection.QueryAsync<Country>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return countries.ToList();
        }

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

                public async Task<bool> AirportCodeExistsAsync(string code, CancellationToken cancellationToken)
        {
            const string sql = "SELECT COUNT(1) FROM airport WHERE code = @Code AND is_deleted = 0;";

            await using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));

            return count > 0;
        }

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
                WHERE a.is_deleted = 0
                  AND (@Search IS NULL
                   OR a.name LIKE '%' + @Search + '%'
                   OR a.code LIKE '%' + @Search + '%'
                   OR ci.name LIKE '%' + @Search + '%')
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
                SELECT id AS Id, name AS Name, code AS Code, city_id AS CityId, is_deleted AS IsDeleted
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

        public async Task<bool> AirportHasPurchasesAsync(int airportId, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM dbo.PurchaseOrderRoute por
                INNER JOIN dbo.[route] r ON r.id = por.RouteId
                WHERE r.departure_airport_id = @AirportId
                   OR r.arrival_airport_id = @AirportId;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { AirportId = airportId }, cancellationToken: cancellationToken));

            return count > 0;
        }

        public async Task SoftDeleteAirportAsync(int airportId, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        "UPDATE dbo.[route] SET is_deleted = 1 WHERE departure_airport_id = @Id OR arrival_airport_id = @Id;",
                        new { Id = airportId }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        "UPDATE dbo.airport SET is_deleted = 1 WHERE id = @Id;",
                        new { Id = airportId }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task HardDeleteAirportAsync(int airportId, CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM dbo.airport WHERE id = @Id;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(sql, new { Id = airportId }, cancellationToken: cancellationToken));
        }
    }
}