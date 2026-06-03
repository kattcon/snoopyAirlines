using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly string _connectionString;

        public BookingRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<Booking> BookAsync(BookingRequest bookingRequest, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QuerySingleAsync<Booking>(
                    new CommandDefinition(
                        "dbo.book",
                        new
                        {
                            purchase_order_id = bookingRequest.PurchaseOrderId,
                            email = bookingRequest.Email,
                            card_brand = bookingRequest.CardBrand,
                            card_last_four = bookingRequest.CardLastFour,
                            card_holder_name = bookingRequest.CardHolderName
                        },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken));
            }
            catch (SqlException exception) when (exception.Number == 50000)
            {
                throw new InvalidOperationException(exception.Message, exception);
            }
        }

        public async Task<Booking?> GetByGuidAsync(Guid bookingGuid, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    guid AS Guid,
                    purchase_order_id AS PurchaseOrderId,
                    flight_guid AS FlightGuid,
                    confirmation_code AS ConfirmationCode,
                    email AS Email,
                    status AS Status,
                    total_amount AS TotalAmount,
                    card_brand AS CardBrand,
                    card_last_four AS CardLastFour,
                    card_holder_name AS CardHolderName,
                    created_at AS CreatedAt,
                    confirmed_at AS ConfirmedAt
                FROM dbo.booking
                WHERE guid = @BookingGuid;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Booking>(
                new CommandDefinition(sql, new { BookingGuid = bookingGuid }, cancellationToken: cancellationToken));
        }
    }
}
