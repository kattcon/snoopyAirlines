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

            var legs = await connection.QueryAsync<FlightReportView>(
                new CommandDefinition(
                    """
                    SELECT * FROM dbo.GetFlightReportByConfirmation(@ConfirmationNumber, @LastNames)
                    ORDER BY SequenceNumber;
                    """,
                        new { ConfirmationNumber = confirmationNumber, LastNames = lastNames },
                        cancellationToken: cancellationToken));
            
            var legsList = legs.ToList();

            var passengers = await connection.QueryAsync<PassengerReportView>(
                new CommandDefinition(
                    """
                    SELECT
                        p.FirstName,
                        p.LastName,
                        p.Gender,
                        p.Nationality,
                        p.Birthday,
                        p.BirthMonth,
                        p.BirthYear,
                        p.CarryOnLuggage,
                        p.CheckedLuggage
                    FROM dbo.Passenger p
                    JOIN dbo.booking b ON p.PurchaseOrderId = b.purchase_order_id
                    WHERE b.confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationNumber)))
                    ORDER BY p.Id;
                    """,
                    new { ConfirmationNumber = confirmationNumber },
                    cancellationToken: cancellationToken));
            
            return new FlightSearchResult
            {
                Legs = legsList,
                Passengers = passengers.ToList()
            };
            
        }
    }
}
