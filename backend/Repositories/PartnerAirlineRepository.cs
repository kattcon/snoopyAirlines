using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain.Airlines;

namespace SnoopyAirlines.Repositories
{
    public class PartnerAirlineRepository : IPartnerAirlineRepository
    {
        private readonly string _connectionString;

        public PartnerAirlineRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<PartnerAirline>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);

            var airlines = await connection.QueryAsync<PartnerAirline>(
                new CommandDefinition(
                    """
                    SELECT
                        id AS Id,
                        name AS Name,
                        host AS Host,
                        api_key AS ApiKey
                    FROM dbo.partner_airline
                    ORDER BY id;
                    """,
                    cancellationToken: cancellationToken));

            return airlines.ToList();
        }
    }
}
