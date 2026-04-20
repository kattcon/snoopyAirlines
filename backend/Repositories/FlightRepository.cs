using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class FlightRepository
    {
        private readonly string _connectionString;

        public FlightRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<Flight>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    airplane_id AS AirplaneId,
                    departure_airport_id AS DepartureAirportId,
                    arrival_airport_id AS ArrivalAirportId,
                    departure_time AS DepartureTime,
                    arrival_time AS ArrivalTime,
                    duration_minutes AS DurationMinutes,
                    price_first_class AS PriceFirstClass,
                    price_economy_class AS PriceEconomyClass,
                    price_carry_on_baggage AS PriceCarryOnBaggage,
                    price_checked_baggage AS PriceCheckedBaggage,
                    weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                    weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                    checked_baggage_price_multiplier AS CheckedBaggagePriceMultiplier
                FROM flight
                ORDER BY departure_time;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var flights = await connection.QueryAsync<Flight>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return flights.ToList();
        }

        public async Task<Flight> CreateAsync(Flight flight, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO flight (
                    airplane_id,
                    departure_airport_id,
                    arrival_airport_id,
                    departure_time,
                    arrival_time,
                    duration_minutes,
                    price_first_class,
                    price_economy_class,
                    price_carry_on_baggage,
                    price_checked_baggage,
                    weight_limit_carry_on_baggage,
                    weight_limit_checked_baggage,
                    checked_baggage_price_multiplier
                )
                OUTPUT
                    INSERTED.id AS Id,
                    INSERTED.airplane_id AS AirplaneId,
                    INSERTED.departure_airport_id AS DepartureAirportId,
                    INSERTED.arrival_airport_id AS ArrivalAirportId,
                    INSERTED.departure_time AS DepartureTime,
                    INSERTED.arrival_time AS ArrivalTime,
                    INSERTED.duration_minutes AS DurationMinutes,
                    INSERTED.price_first_class AS PriceFirstClass,
                    INSERTED.price_economy_class AS PriceEconomyClass,
                    INSERTED.price_carry_on_baggage AS PriceCarryOnBaggage,
                    INSERTED.price_checked_baggage AS PriceCheckedBaggage,
                    INSERTED.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                    INSERTED.weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                    INSERTED.checked_baggage_price_multiplier AS CheckedBaggagePriceMultiplier
                VALUES (
                    @AirplaneId,
                    @DepartureAirportId,
                    @ArrivalAirportId,
                    @DepartureTime,
                    @ArrivalTime,
                    @DurationMinutes,
                    @PriceFirstClass,
                    @PriceEconomyClass,
                    @PriceCarryOnBaggage,
                    @PriceCheckedBaggage,
                    @WeightLimitCarryOnBaggage,
                    @WeightLimitCheckedBaggage,
                    @CheckedBaggagePriceMultiplier
                );
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<Flight>(
                new CommandDefinition(sql, flight, cancellationToken: cancellationToken));
        }
    }
}
