using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.FlightCustomerReport;

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

        public async Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(string confirmationNumber, string lastNames, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var flightSearch = await connection.QuerySingleOrDefaultAsync<FlightCustomerReport>("""
                SELECT * FROM dbo.GetFlightReportByConfirmation(@ConfirmationNumber, @LastNames);
                """, new { ConfirmationNumber = confirmationNumber, LastNames = lastNames });

            return flightSearch;
        }
    }
}
