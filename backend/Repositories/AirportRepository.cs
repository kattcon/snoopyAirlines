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
            const string sql = "SELECT COUNT(1) FROM airport WHERE code = @Code AND deleted_at IS NULL;";

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
                WHERE a.deleted_at IS NULL
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
                SELECT id AS Id, name AS Name, code AS Code, city_id AS CityId, deleted_at AS DeletedAt
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
            const string sql = "UPDATE airport SET deleted_at = SYSUTCDATETIME() WHERE id = @Id AND deleted_at IS NULL;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(sql, new { Id = airportId }, cancellationToken: cancellationToken));
        }

        public async Task HardDeleteAirportAsync(int airportId, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await connection.ExecuteAsync(
                    new CommandDefinition("""
                        DELETE dbo.itinerary
                        FROM dbo.itinerary it
                        INNER JOIN dbo.flight f ON f.guid = it.flight_guid
                        INNER JOIN dbo.[route] r ON r.id = f.route_id
                        WHERE r.departure_airport_id = @AirportId
                           OR r.arrival_airport_id = @AirportId;
                        """, new { AirportId = airportId }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition("""
                        DELETE dbo.flight
                        FROM dbo.flight f
                        INNER JOIN dbo.[route] r ON r.id = f.route_id
                        WHERE r.departure_airport_id = @AirportId
                           OR r.arrival_airport_id = @AirportId;
                        """, new { AirportId = airportId }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition("""
                        DELETE FROM dbo.[route]
                        WHERE departure_airport_id = @AirportId
                           OR arrival_airport_id = @AirportId;
                        """, new { AirportId = airportId }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition("""
                        DELETE FROM dbo.airport
                        WHERE id = @AirportId;
                        """, new { AirportId = airportId }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}