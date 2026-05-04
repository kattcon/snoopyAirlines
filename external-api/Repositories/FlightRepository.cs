using System.Globalization;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Repositories
{
    public class FlightRepository
    {
        private static readonly Guid FlightGuidNamespace = new("9e991ddc-7c58-4e29-a379-503fc594c1e2");

        private readonly string _connectionString;

        public FlightRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<Flight>> GetFlights(
            FlightQuery? flightQuery,
            CancellationToken cancellationToken = default)
        {
            flightQuery ??= new FlightQuery();

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

            if (flightQuery.EarliestDeparture is not null)
            {
                if (flightQuery.LatestDeparture is not null)
                {
                    AddDepartureWindowFilter(
                        where,
                        parameters,
                        flightQuery.EarliestDeparture.Value,
                        flightQuery.LatestDeparture.Value);
                }
                else
                {
                    where.Add("f.departure_time >= @EarliestDepartureTime");
                    parameters.Add("EarliestDepartureTime", flightQuery.EarliestDeparture.Value.TimeOfDay);
                }
            }
            else if (flightQuery.LatestDeparture is not null)
            {
                where.Add("f.departure_time <= @LatestDepartureTime");
                parameters.Add("LatestDepartureTime", flightQuery.LatestDeparture.Value.TimeOfDay);
            }

            if (where.Count > 0)
            {
                sql.AppendLine("WHERE " + string.Join(" AND ", where));
            }

            sql.AppendLine("ORDER BY f.departure_time;");

            await using var connection = new SqlConnection(_connectionString);
            var flights = await connection.QueryAsync<FlightRecord>(
                new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken));

            return flights.Select(ToFlight).ToList();
        }

        private static Flight ToFlight(FlightRecord flight)
        {
            return new Flight
            {
                FlightGUID = CreateFlightGuid(flight.Id),
                DepartureTime = FormatTime(flight.DepartureTime),
                ArrivalTime = FormatTime(flight.ArrivalTime),
                Frequency = FlightFrequency.FromByte(flight.Frequency),
                Duration = FormatDuration(flight.DurationMinutes),
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

        private static string CreateFlightGuid(int flightId)
        {
            var bytes = FlightGuidNamespace.ToByteArray();
            var idBytes = BitConverter.GetBytes(flightId);
            Array.Copy(idBytes, 0, bytes, bytes.Length - idBytes.Length, idBytes.Length);

            return new Guid(bytes).ToString();
        }

        private static void AddDepartureWindowFilter(
            ICollection<string> where,
            DynamicParameters parameters,
            DateTime earliestDeparture,
            DateTime latestDeparture)
        {
            if (latestDeparture - earliestDeparture >= TimeSpan.FromDays(7))
            {
                return;
            }

            var conditions = new List<string>();
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;
            var index = 0;

            while (currentDate <= endDate)
            {
                var frequencyParameter = $"FrequencyMask{index}";
                var startTimeParameter = $"StartTime{index}";
                var endTimeParameter = $"EndTime{index}";
                var startTime = currentDate == earliestDeparture.Date
                    ? earliestDeparture.TimeOfDay
                    : TimeSpan.Zero;
                var endTime = currentDate == latestDeparture.Date
                    ? latestDeparture.TimeOfDay
                    : new TimeSpan(23, 59, 59);

                conditions.Add(
                    $"((f.frequency & @{frequencyParameter}) <> 0 AND f.departure_time >= @{startTimeParameter} AND f.departure_time <= @{endTimeParameter})");
                parameters.Add(frequencyParameter, GetFrequencyMask(currentDate.DayOfWeek));
                parameters.Add(startTimeParameter, startTime);
                parameters.Add(endTimeParameter, endTime);

                currentDate = currentDate.AddDays(1);
                index++;
            }

            where.Add("(" + string.Join(" OR ", conditions) + ")");
        }

        private static byte GetFrequencyMask(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => 0b0100_0000,
                DayOfWeek.Tuesday => 0b0010_0000,
                DayOfWeek.Wednesday => 0b0001_0000,
                DayOfWeek.Thursday => 0b0000_1000,
                DayOfWeek.Friday => 0b0000_0100,
                DayOfWeek.Saturday => 0b0000_0010,
                DayOfWeek.Sunday => 0b0000_0001,
                _ => 0
            };
        }

        private static string FormatTime(TimeSpan time)
        {
            return time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }

        private static string FormatDuration(int durationMinutes)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;

            return $"{hours:00}:{minutes:00}";
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
