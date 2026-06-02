using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.External.Domain;
using DomainRoute = SnoopyAirlines.External.Domain.Route;

namespace SnoopyAirlines.External.Repositories
{
    public class RouteRepository
    {
        private readonly string _connectionString;

        public RouteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<DomainRoute>> GetRoutes(
            RouteSearchQuery? routeQuery,
            CancellationToken cancellationToken = default)
        {
            routeQuery ??= new RouteSearchQuery();

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
                FROM [route] f
                INNER JOIN airplane plane ON f.airplane_id = plane.id
                INNER JOIN airport departure_airport ON f.departure_airport_id = departure_airport.id
                INNER JOIN city departure_city ON departure_airport.city_id = departure_city.id
                INNER JOIN airport arrival_airport ON f.arrival_airport_id = arrival_airport.id
                INNER JOIN city arrival_city ON arrival_airport.city_id = arrival_city.id
                """);
            sql.AppendLine();

            var where = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(routeQuery.Destination))
            {
                where.Add("arrival_airport.code = @Destination");
                parameters.Add("Destination", routeQuery.Destination);
            }

            if (routeQuery.QuantityOfPassengers.HasValue)
            {
                where.Add("((plane.tourist_rows * plane.tourist_columns) + (plane.firstClass_rows * plane.firstClass_columns)) >= @QuantityOfPassengers");
                parameters.Add("QuantityOfPassengers", routeQuery.QuantityOfPassengers.Value);
            }

            if (routeQuery.ArrivalWindows.Count > 0)
            {
                AddArrivalWindowFilters(where, parameters, routeQuery.ArrivalWindows);
            }

            if (where.Count > 0)
            {
                sql.AppendLine("WHERE " + string.Join(" AND ", where));
            }

            sql.AppendLine("ORDER BY f.arrival_time, f.departure_time;");

            await using var connection = new SqlConnection(_connectionString);
            var routes = await connection.QueryAsync<RouteRecord>(
                new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken));

            return routes.Select(ToRoute).ToList();
        }

        private static DomainRoute ToRoute(RouteRecord route)
        {
            return new DomainRoute
            {
                Id = route.Id,
                DepartureTime = TimeOnly.FromTimeSpan(route.DepartureTime),
                ArrivalTime = TimeOnly.FromTimeSpan(route.ArrivalTime),
                Frequency = RouteFrequency.FromByte(route.Frequency),
                DurationMinutes = route.DurationMinutes,
                DepartureAirport = new Airport
                {
                    Code = route.DepartureAirportCode,
                    Name = route.DepartureAirportName,
                    City = route.DepartureAirportCity
                },
                ArrivalAirport = new Airport
                {
                    Code = route.ArrivalAirportCode,
                    Name = route.ArrivalAirportName,
                    City = route.ArrivalAirportCity
                },
                TouristPrice = route.TouristPrice,
                FirstClassPrice = route.FirstClassPrice,
                CarryOnPrice = route.CarryOnPrice,
                CheckedPrice = route.CheckedPrice
            };
        }

        private static void AddArrivalWindowFilters(
            ICollection<string> where,
            DynamicParameters parameters,
            IReadOnlyCollection<RouteArrivalWindow> arrivalWindows)
        {
            var conditions = new List<string>();
            var index = 0;

            foreach (var arrivalWindow in arrivalWindows)
            {
                var sameDayFrequencyParameter = $"SameDayFrequencyMask{index}";
                var previousDayFrequencyParameter = $"PreviousDayFrequencyMask{index}";
                var startTimeParameter = $"StartTime{index}";
                var endTimeParameter = $"EndTime{index}";
                var sameDayFrequencyMask = ToByte(arrivalWindow.SameDayDepartureFrequency);
                var previousDayFrequencyMask = ToByte(arrivalWindow.PreviousDayDepartureFrequency);
                var windowConditions = new List<string>();

                if (sameDayFrequencyMask != 0)
                {
                    windowConditions.Add(
                        $"((f.frequency & @{sameDayFrequencyParameter}) <> 0 AND f.arrival_time >= @{startTimeParameter} AND f.arrival_time <= @{endTimeParameter} AND f.arrival_time >= f.departure_time)");
                    parameters.Add(sameDayFrequencyParameter, sameDayFrequencyMask);
                }

                if (previousDayFrequencyMask != 0)
                {
                    windowConditions.Add(
                        $"((f.frequency & @{previousDayFrequencyParameter}) <> 0 AND f.arrival_time >= @{startTimeParameter} AND f.arrival_time <= @{endTimeParameter} AND f.arrival_time < f.departure_time)");
                    parameters.Add(previousDayFrequencyParameter, previousDayFrequencyMask);
                }

                if (windowConditions.Count == 0)
                {
                    continue;
                }

                conditions.Add("(" + string.Join(" OR ", windowConditions) + ")");
                parameters.Add(startTimeParameter, arrivalWindow.EarliestArrival.ToTimeSpan());
                parameters.Add(endTimeParameter, arrivalWindow.LatestArrival.ToTimeSpan());

                index++;
            }

            if (conditions.Count > 0)
            {
                where.Add("(" + string.Join(" OR ", conditions) + ")");
            }
        }

        private static byte ToByte(RouteFrequency frequency)
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

        private class RouteRecord
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
