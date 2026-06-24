using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Repositories;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{

    public class FlightLuggageRepository : IFlightLuggageRepository
    {

        private readonly string _connectionString;

        public FlightLuggageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<FlightLuggageView> GetFlightLuggageInfoByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            var commandDefinition = new CommandDefinition("""
            SELECT * FROM dbo.GetBaggageInfoByConfirmation(@ConfirmationNumber)
            """, new { ConfirmationNumber = confirmationNumber }, cancellationToken: cancellationToken
            );
            var flightLuggageInfo = await connection.QueryFirstOrDefaultAsync<FlightLuggageView>(commandDefinition);
            return flightLuggageInfo;
        }
    }
}