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
                    frequency AS Frequency,
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
            var flights = await connection.QueryAsync<FlightRecord>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return flights.Select(ToFlight).ToList();
        }

        public async Task<IReadOnlyCollection<Flight>> SearchAsync(
            int? departureAirportId,
            int? arrivalAirportId,
            DateOnly? departureDate,
            CancellationToken cancellationToken)
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
                WHERE (@DepartureAirportId IS NULL OR departure_airport_id = @DepartureAirportId)
                  AND (@ArrivalAirportId IS NULL OR arrival_airport_id = @ArrivalAirportId)
                  AND (@DepartureDate IS NULL OR CAST(departure_time AS date) = @DepartureDate)
                ORDER BY departure_time;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var flights = await connection.QueryAsync<Flight>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        DepartureAirportId = departureAirportId,
                        ArrivalAirportId = arrivalAirportId,
                        DepartureDate = departureDate?.ToDateTime(TimeOnly.MinValue)
                    },
                    cancellationToken: cancellationToken));

            return flights.ToList();
        }

        public async Task<Flight> SaveAsync(Flight flight, CancellationToken cancellationToken)
        {
            const string sql = """
                IF @Id > 0 AND EXISTS (SELECT 1 FROM flight WHERE id = @Id)
                BEGIN
                    UPDATE flight
                    SET
                        airplane_id = @AirplaneId,
                        departure_airport_id = @DepartureAirportId,
                        arrival_airport_id = @ArrivalAirportId,
                        departure_time = @DepartureTime,
                        arrival_time = @ArrivalTime,
                        frequency = @Frequency,
                        duration_minutes = @DurationMinutes,
                        price_first_class = @PriceFirstClass,
                        price_economy_class = @PriceEconomyClass,
                        price_carry_on_baggage = @PriceCarryOnBaggage,
                        price_checked_baggage = @PriceCheckedBaggage,
                        weight_limit_carry_on_baggage = @WeightLimitCarryOnBaggage,
                        weight_limit_checked_baggage = @WeightLimitCheckedBaggage,
                        checked_baggage_price_multiplier = @CheckedBaggagePriceMultiplier
                    OUTPUT
                        INSERTED.id AS Id,
                        INSERTED.airplane_id AS AirplaneId,
                        INSERTED.departure_airport_id AS DepartureAirportId,
                        INSERTED.arrival_airport_id AS ArrivalAirportId,
                        INSERTED.departure_time AS DepartureTime,
                        INSERTED.arrival_time AS ArrivalTime,
                        INSERTED.frequency AS Frequency,
                        INSERTED.duration_minutes AS DurationMinutes,
                        INSERTED.price_first_class AS PriceFirstClass,
                        INSERTED.price_economy_class AS PriceEconomyClass,
                        INSERTED.price_carry_on_baggage AS PriceCarryOnBaggage,
                        INSERTED.price_checked_baggage AS PriceCheckedBaggage,
                        INSERTED.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                        INSERTED.weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                        INSERTED.checked_baggage_price_multiplier AS CheckedBaggagePriceMultiplier
                    WHERE id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO flight (
                        airplane_id,
                        departure_airport_id,
                        arrival_airport_id,
                        departure_time,
                        arrival_time,
                        frequency,
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
                        INSERTED.frequency AS Frequency,
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
                        @Frequency,
                        @DurationMinutes,
                        @PriceFirstClass,
                        @PriceEconomyClass,
                        @PriceCarryOnBaggage,
                        @PriceCheckedBaggage,
                        @WeightLimitCarryOnBaggage,
                        @WeightLimitCheckedBaggage,
                        @CheckedBaggagePriceMultiplier
                    );
                END
                """;

            await using var connection = new SqlConnection(_connectionString);
            var savedFlight = await connection.QuerySingleAsync<FlightRecord>(
                new CommandDefinition(sql, ToParameters(flight), cancellationToken: cancellationToken));

            return ToFlight(savedFlight);
        }

        private static Flight ToFlight(FlightRecord flight)
        {
            return new Flight
            {
                Id = flight.Id,
                AirplaneId = flight.AirplaneId,
                DepartureAirportId = flight.DepartureAirportId,
                ArrivalAirportId = flight.ArrivalAirportId,
                DepartureTime = TimeOnly.FromTimeSpan(flight.DepartureTime),
                ArrivalTime = TimeOnly.FromTimeSpan(flight.ArrivalTime),
                Frequency = FlightFrequency.FromByte(flight.Frequency),
                DurationMinutes = flight.DurationMinutes,
                PriceFirstClass = flight.PriceFirstClass,
                PriceEconomyClass = flight.PriceEconomyClass,
                PriceCarryOnBaggage = flight.PriceCarryOnBaggage,
                PriceCheckedBaggage = flight.PriceCheckedBaggage,
                WeightLimitCarryOnBaggage = flight.WeightLimitCarryOnBaggage,
                WeightLimitCheckedBaggage = flight.WeightLimitCheckedBaggage,
                CheckedBaggagePriceMultiplier = flight.CheckedBaggagePriceMultiplier
            };
        }

        private static object ToParameters(Flight flight)
        {
            return new
            {
                flight.Id,
                flight.AirplaneId,
                flight.DepartureAirportId,
                flight.ArrivalAirportId,
                DepartureTime = flight.DepartureTime.ToTimeSpan(),
                ArrivalTime = flight.ArrivalTime.ToTimeSpan(),
                Frequency = flight.Frequency.ToByte(),
                flight.DurationMinutes,
                flight.PriceFirstClass,
                flight.PriceEconomyClass,
                flight.PriceCarryOnBaggage,
                flight.PriceCheckedBaggage,
                flight.WeightLimitCarryOnBaggage,
                flight.WeightLimitCheckedBaggage,
                flight.CheckedBaggagePriceMultiplier
            };
        }

        private class FlightRecord
        {
            public int Id { get; set; }
            public int AirplaneId { get; set; }
            public int DepartureAirportId { get; set; }
            public int ArrivalAirportId { get; set; }
            public TimeSpan DepartureTime { get; set; }
            public TimeSpan ArrivalTime { get; set; }
            public byte Frequency { get; set; }
            public int DurationMinutes { get; set; }
            public decimal PriceFirstClass { get; set; }
            public decimal PriceEconomyClass { get; set; }
            public decimal PriceCarryOnBaggage { get; set; }
            public decimal PriceCheckedBaggage { get; set; }
            public int WeightLimitCarryOnBaggage { get; set; }
            public int WeightLimitCheckedBaggage { get; set; }
            public decimal CheckedBaggagePriceMultiplier { get; set; }
        }
    }
}
