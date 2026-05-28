using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Repositories
{
    public class AirportRepository
    {
        private readonly string _connectionString;

        public AirportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<Airport>> GetAllAirports(
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.code AS Code,
                    a.name AS Name,
                    c.name AS City
                FROM airport a
                INNER JOIN city c ON a.city_id = c.id
                ORDER BY a.code
            ";

            await using var connection = new SqlConnection(_connectionString);
            var airports = await connection.QueryAsync<Airport>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return airports.ToList();
        }

        public async Task<Airport?> GetAirportByCode(
            string code,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    a.code AS Code,
                    a.name AS Name,
                    c.name AS City
                FROM airport a
                INNER JOIN city c ON a.city_id = c.id
                WHERE a.code = @Code
            ";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Airport>(
                new CommandDefinition(
                    sql,
                    new { Code = code.ToUpperInvariant() },
                    cancellationToken: cancellationToken
                )
            );
        }
    }
}
