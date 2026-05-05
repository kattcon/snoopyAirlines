using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Repositories
{
    public class FlightRepository
    {
        private readonly string _connectionString;

        public FlightRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<FlightDefinition>> GetFlightDefinitions(
            FlightDefinitionQuery? flightQuery,
            CancellationToken cancellationToken = default)
        {
            flightQuery ??= new FlightDefinitionQuery();

            var sql = new StringBuilder("""
                SELECT
                    f.id AS Id,
                    f.departure_time AS DepartureTime,
                    f.arrival_time AS ArrivalTime,
                    f.frequency AS Frequency,
                    f.duration_minutes AS DurationMinutes,
                    departure_airport.code AS DepartureAirportCode,
                    departure_airport.name AS DepartureAirportName,
                    departure_city.name AS DepartureAirportCity,
                    arrival_airport.code AS ArrivalAirportCode,
                    arrival_airport.name AS ArrivalAirportName,
                    arrival_city.name AS ArrivalAirportCity,
                    f.price_economy_class AS TouristPrice,
                    f.price_first_class AS FirstClassPrice,
                    f.price_carry_on_baggage AS CarryOnPrice,
                    f.price_checked_baggage AS CheckedPrice
                FROM flight f
                INNER JOIN airport departure_airport ON f.departure_airport_id = departure_airport.id
                INNER JOIN city departure_city ON departure_airport.city_id = departure_city.id
                INNER JOIN airport arrival_airport ON f.arrival_airport_id = arrival_airport.id
                INNER JOIN city arrival_city ON arrival_airport.city_id = arrival_city.id
                """);
            sql.AppendLine();

            var where = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(flightQuery.Origin))
            {
                where.Add("departure_airport.code = @Origin");
                parameters.Add("Origin", flightQuery.Origin);
            }

            if (!string.IsNullOrWhiteSpace(flightQuery.Destination))
            {
                where.Add("arrival_airport.code = @Destination");
                parameters.Add("Destination", flightQuery.Destination);
            }

            if (flightQuery.DepartureWindows.Count > 0)
            {
                AddDepartureWindowFilters(where, parameters, flightQuery.DepartureWindows);
            }

            if (where.Count > 0)
            {
                sql.AppendLine("WHERE " + string.Join(" AND ", where));
            }

            sql.AppendLine("ORDER BY f.departure_time;");

            await using var connection = new SqlConnection(_connectionString);
            var flights = await connection.QueryAsync<FlightRecord>(
                new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken));

            return flights.Select(ToFlightDefinition).ToList();
        }

        private static FlightDefinition ToFlightDefinition(FlightRecord flight)
        {
            return new FlightDefinition
            {
                Id = flight.Id,
                DepartureTime = TimeOnly.FromTimeSpan(flight.DepartureTime),
                ArrivalTime = TimeOnly.FromTimeSpan(flight.ArrivalTime),
                Frequency = FlightFrequency.FromByte(flight.Frequency),
                DurationMinutes = flight.DurationMinutes,
                DepartureAirport = new Airport
                {
                    Code = flight.DepartureAirportCode,
                    Name = flight.DepartureAirportName,
                    City = flight.DepartureAirportCity
                },
                ArrivalAirport = new Airport
                {
                    Code = flight.ArrivalAirportCode,
                    Name = flight.ArrivalAirportName,
                    City = flight.ArrivalAirportCity
                },
                TouristPrice = flight.TouristPrice,
                FirstClassPrice = flight.FirstClassPrice,
                CarryOnPrice = flight.CarryOnPrice,
                CheckedPrice = flight.CheckedPrice
            };
        }

        private static void AddDepartureWindowFilters(
            ICollection<string> where,
            DynamicParameters parameters,
            IReadOnlyCollection<FlightDefinitionDepartureWindow> departureWindows)
        {
            var conditions = new List<string>();
            var index = 0;

            foreach (var departureWindow in departureWindows)
            {
                var frequencyParameter = $"FrequencyMask{index}";
                var startTimeParameter = $"StartTime{index}";
                var endTimeParameter = $"EndTime{index}";
                var frequencyMask = ToByte(departureWindow.Frequency);

                if (frequencyMask == 0)
                {
                    continue;
                }

                conditions.Add(
                    $"((f.frequency & @{frequencyParameter}) <> 0 AND f.departure_time >= @{startTimeParameter} AND f.departure_time <= @{endTimeParameter})");
                parameters.Add(frequencyParameter, frequencyMask);
                parameters.Add(startTimeParameter, departureWindow.EarliestDeparture.ToTimeSpan());
                parameters.Add(endTimeParameter, departureWindow.LatestDeparture.ToTimeSpan());

                index++;
            }

            if (conditions.Count > 0)
            {
                where.Add("(" + string.Join(" OR ", conditions) + ")");
            }
        }

        private static byte ToByte(FlightFrequency frequency)
        {
            byte value = 0;

            if (frequency.Monday) value |= 0b0100_0000;
            if (frequency.Tuesday) value |= 0b0010_0000;
            if (frequency.Wednesday) value |= 0b0001_0000;
            if (frequency.Thursday) value |= 0b0000_1000;
            if (frequency.Friday) value |= 0b0000_0100;
            if (frequency.Saturday) value |= 0b0000_0010;
            if (frequency.Sunday) value |= 0b0000_0001;

            return value;
        }

        private class FlightRecord
        {
            public int Id { get; set; }
            public TimeSpan DepartureTime { get; set; }
            public TimeSpan ArrivalTime { get; set; }
            public byte Frequency { get; set; }
            public int DurationMinutes { get; set; }
            required public string DepartureAirportCode { get; set; }
            required public string DepartureAirportName { get; set; }
            required public string DepartureAirportCity { get; set; }
            required public string ArrivalAirportCode { get; set; }
            required public string ArrivalAirportName { get; set; }
            required public string ArrivalAirportCity { get; set; }
            public decimal TouristPrice { get; set; }
            public decimal FirstClassPrice { get; set; }
            public decimal CarryOnPrice { get; set; }
            public decimal CheckedPrice { get; set; }
        }
    }
}
