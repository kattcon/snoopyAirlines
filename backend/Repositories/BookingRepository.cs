using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class BookingRepository
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
    }
}
