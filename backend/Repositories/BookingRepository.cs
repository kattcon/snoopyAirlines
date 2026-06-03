using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;
using snoopy_airlines_backend.Domain;

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
            await connection.OpenAsync(cancellationToken);

            try
            {
                var booking = await connection.QuerySingleAsync<Booking>(
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

                booking.Itinerary = await GetItineraryAsync(connection, booking.Guid, cancellationToken);
                return booking;
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

                SELECT
                    booking_guid AS BookingGuid,
                    sequence_number AS SequenceNumber,
                    flight_guid AS FlightGuid
                FROM dbo.itinerary
                WHERE booking_guid = @BookingGuid
                ORDER BY sequence_number;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition(sql, new { BookingGuid = bookingGuid }, cancellationToken: cancellationToken));

            var booking = await results.ReadSingleOrDefaultAsync<Booking>();
            if (booking is null)
            {
                return null;
            }

            var itinerary = await results.ReadAsync<ItineraryLeg>();
            booking.Itinerary = itinerary.ToList();

            return booking;
        }

        private static async Task<List<ItineraryLeg>> GetItineraryAsync(
            SqlConnection connection,
            Guid bookingGuid,
            CancellationToken cancellationToken)
        {
            var itinerary = await connection.QueryAsync<ItineraryLeg>(
                new CommandDefinition("""
                    SELECT
                        booking_guid AS BookingGuid,
                        sequence_number AS SequenceNumber,
                        flight_guid AS FlightGuid
                    FROM dbo.itinerary
                    WHERE booking_guid = @BookingGuid
                    ORDER BY sequence_number;
                    """, new { BookingGuid = bookingGuid }, cancellationToken: cancellationToken));

            return itinerary.ToList();
        }

        public async Task<PurchaseOrderEmailData?> GetBookingItineraryDetailsAsync(Guid bookingGuid, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var row = await connection.QuerySingleOrDefaultAsync<PurchaseOrderDetailRow>("""
                SELECT * FROM dbo.GetBookingItineraryDetails(@BookingGuid);
                """, new { BookingGuid = bookingGuid });

            if (row is null)
            {
                return null;
            }

            var passengers = await connection.QueryAsync<PassengerEmailData>("""
                SELECT
                    p.FirstName,
                    p.LastName,
                    p.Gender,
                    p.Nationality,
                    p.BirthDay,
                    p.BirthMonth,
                    p.BirthYear
                FROM dbo.Passenger p
                JOIN dbo.booking b ON p.PurchaseOrderId = b.PurchaseOrderId
                WHERE b.Guid = @BookingGuid
                """, new { BookingGuid = bookingGuid });
            
            return new PurchaseOrderEmailData
            {
                BookingGuid = row.BookingGuid,
                ConfirmationCode = row.ConfirmationCode,
                Email = row.Email,
                BookingStatus = row.BookingStatus,
                TotalAmount = row.TotalAmount,
                CardBrand = row.CardBrand,
                CardLastFour = row.CardLastFour,
                CardHolderName = row.CardHolderName,
                CreatedAt = row.CreatedAt,
                ConfirmedAt = row.ConfirmedAt,
                PurchaseOrderId = row.PurchaseOrderId,
                SeatClass = row.SeatClass,
                DepartureAirportName = row.DepartureAirportName,
                DepartureAirportCode = row.DepartureAirportCode,
                DepartureCityName = row.DepartureCityName,
                DepartureAt = row.DepartureAt,
                ArrivalAirportName = row.ArrivalAirportName,
                ArrivalAirportCode = row.ArrivalAirportCode,
                ArrivalCityName = row.ArrivalCityName,
                ArrivalAt = row.ArrivalAt,
                TotalItineraryMinutes = row.TotalItineraryMinutes,
                TotalFlightMinutes = row.TotalFlightMinutes,
                LayoverCount = row.LayoverCount,
                Passengers = passengers.ToList()
            };    
        }
    }
}
