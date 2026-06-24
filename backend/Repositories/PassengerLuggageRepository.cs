using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain.View;
using SnoopyAirlines.Domain.View;

namespace snoopy_airlines_backend.Repositories
{
    public class PassengerLuggageRepository : IPassengerLuggageRepository
    {
        private readonly string _connectionString;

        public PassengerLuggageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            var commandDefinition = new CommandDefinition("""
                SELECT * FROM dbo.GetPassengersByConfirmation(@ConfirmationNumber)
                """, new { ConfirmationNumber = confirmationNumber }, cancellationToken: cancellationToken
            );
            var passengers = await connection.QueryAsync<PassengerView>(commandDefinition);
            return passengers.ToList();
        }
    }
}
