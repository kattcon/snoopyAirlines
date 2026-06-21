using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class FlightSearchRepository : IFlightSearchRepository
    {
        private readonly string _connectionString;

        public FlightSearchRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var CommandDefinition = new CommandDefinition(
                """
                SELECT * FROM dbo.GetFlightReportByConfirmation(@ConfirmationNumber, @LastNames)
                ORDER BY SequenceNumber;
                """,
                new { ConfirmationNumber = confirmationNumber, LastNames = lastNames },
                cancellationToken: cancellationToken);

            var flightReport = await connection.QueryAsync<FlightReportView>(CommandDefinition);

            return flightReport.ToList();
        }
    }
}
