using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly string _connectionString;

        public RouteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<DomainRoute>> GetAllAsync(CancellationToken cancellationToken)
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
                FROM [route]
                ORDER BY departure_time;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var routes = await connection.QueryAsync<RouteRecord>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return routes.Select(ToRoute).ToList();
        }

        public async Task<IReadOnlyCollection<DomainRoute>> GetRoutesAsync(
            RouteSearchQuery routeQuery,
            CancellationToken cancellationToken = default)
        {
            routeQuery ??= new RouteSearchQuery();

            var sql = new StringBuilder();
            sql.Append("""
                SELECT
                    f.id AS Id,
                    f.airplane_id AS AirplaneId,
                    f.departure_airport_id AS DepartureAirportId,
                    f.arrival_airport_id AS ArrivalAirportId,
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
                    f.price_economy_class AS PriceEconomyClass,
                    f.price_first_class AS PriceFirstClass,
                    f.price_carry_on_baggage AS PriceCarryOnBaggage,
                    f.price_checked_baggage AS PriceCheckedBaggage,
                    f.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                    f.weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                    f.checked_baggage_price_multiplier AS CheckedBaggagePriceMultiplier
                FROM [route] f
                INNER JOIN airport departure_airport ON f.departure_airport_id = departure_airport.id
                INNER JOIN city departure_city ON departure_airport.city_id = departure_city.id
                INNER JOIN airport arrival_airport ON f.arrival_airport_id = arrival_airport.id
                INNER JOIN city arrival_city ON arrival_airport.city_id = arrival_city.id
                """);

            var where = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(routeQuery.Origin))
            {
                where.Add("departure_airport.code = @Origin");
                parameters.Add("Origin", routeQuery.Origin);
            }

            if (!string.IsNullOrWhiteSpace(routeQuery.Destination))
            {
                where.Add("arrival_airport.code = @Destination");
                parameters.Add("Destination", routeQuery.Destination);
            }

            if (routeQuery.DepartureWindows.Count > 0)
            {
                AddDepartureWindowFilters(where, parameters, routeQuery.DepartureWindows);
            }

           if (where.Count > 0)
            {
                sql.Append(" WHERE ");
                sql.Append(string.Join(" AND ", where));
            }

            sql.Append(" ORDER BY f.departure_time;");

            await using var connection = new SqlConnection(_connectionString);
            var routes = await connection.QueryAsync<RouteSearchRecord>(
                new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken));

            return routes.Select(ToRoute).ToList();
        }

        public async Task<DomainRoute?> GetByIdAsync(int routeId, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    f.id AS Id,
                    f.airplane_id AS AirplaneId,
                    f.departure_airport_id AS DepartureAirportId,
                    f.arrival_airport_id AS ArrivalAirportId,
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
                    f.price_economy_class AS PriceEconomyClass,
                    f.price_first_class AS PriceFirstClass,
                    f.price_carry_on_baggage AS PriceCarryOnBaggage,
                    f.price_checked_baggage AS PriceCheckedBaggage,
                    f.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                    f.weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                    f.checked_baggage_price_multiplier AS CheckedBaggagePriceMultiplier
                FROM [route] f
                INNER JOIN airport departure_airport ON f.departure_airport_id = departure_airport.id
                INNER JOIN city departure_city ON departure_airport.city_id = departure_city.id
                INNER JOIN airport arrival_airport ON f.arrival_airport_id = arrival_airport.id
                INNER JOIN city arrival_city ON arrival_airport.city_id = arrival_city.id
                WHERE f.id = @RouteId;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var route = await connection.QuerySingleOrDefaultAsync<RouteSearchRecord>(
                new CommandDefinition(sql, new { RouteId = routeId }, cancellationToken: cancellationToken));

            return route is null ? null : ToRoute(route);
        }

        public async Task<IReadOnlyCollection<DomainRoute>> SearchAsync(
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
                FROM [route]
                WHERE (@DepartureAirportId IS NULL OR departure_airport_id = @DepartureAirportId)
                  AND (@ArrivalAirportId IS NULL OR arrival_airport_id = @ArrivalAirportId)
                  AND (@DepartureDate IS NULL OR CAST(departure_time AS date) = @DepartureDate)
                ORDER BY departure_time;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var routes = await connection.QueryAsync<DomainRoute>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        DepartureAirportId = departureAirportId,
                        ArrivalAirportId = arrivalAirportId,
                        DepartureDate = departureDate?.ToDateTime(TimeOnly.MinValue)
                    },
                    cancellationToken: cancellationToken));

            return routes.ToList();
        }

        public async Task SaveAsync(DomainRoute route, CancellationToken cancellationToken)
        {
            const string sql = """
                IF @Id > 0 AND EXISTS (SELECT 1 FROM [route] WHERE id = @Id)
                BEGIN
                    UPDATE [route]
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
                    WHERE id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO [route] (
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
            await connection.ExecuteAsync(
                new CommandDefinition(sql, ToParameters(route), cancellationToken: cancellationToken));
        }

        private static DomainRoute ToRoute(RouteRecord route)
        {
            return new DomainRoute
            {
                Id = route.Id,
                AirplaneId = route.AirplaneId,
                DepartureAirportId = route.DepartureAirportId,
                ArrivalAirportId = route.ArrivalAirportId,
                DepartureTime = TimeOnly.FromTimeSpan(route.DepartureTime),
                ArrivalTime = TimeOnly.FromTimeSpan(route.ArrivalTime),
                Frequency = RouteFrequency.FromByte(route.Frequency),
                DurationMinutes = route.DurationMinutes,
                PriceFirstClass = route.PriceFirstClass,
                PriceEconomyClass = route.PriceEconomyClass,
                PriceCarryOnBaggage = route.PriceCarryOnBaggage,
                PriceCheckedBaggage = route.PriceCheckedBaggage,
                WeightLimitCarryOnBaggage = route.WeightLimitCarryOnBaggage,
                WeightLimitCheckedBaggage = route.WeightLimitCheckedBaggage,
                CheckedBaggagePriceMultiplier = route.CheckedBaggagePriceMultiplier
            };
        }

        private static DomainRoute ToRoute(RouteSearchRecord route)
        {
            return new DomainRoute
            {
                Id = route.Id,
                AirplaneId = route.AirplaneId,
                DepartureAirportId = route.DepartureAirportId,
                ArrivalAirportId = route.ArrivalAirportId,
                DepartureTime = TimeOnly.FromTimeSpan(route.DepartureTime),
                ArrivalTime = TimeOnly.FromTimeSpan(route.ArrivalTime),
                Frequency = RouteFrequency.FromByte(route.Frequency),
                DurationMinutes = route.DurationMinutes,
                PriceEconomyClass = route.PriceEconomyClass,
                PriceFirstClass = route.PriceFirstClass,
                PriceCarryOnBaggage = route.PriceCarryOnBaggage,
                PriceCheckedBaggage = route.PriceCheckedBaggage,
                WeightLimitCarryOnBaggage = route.WeightLimitCarryOnBaggage,
                WeightLimitCheckedBaggage = route.WeightLimitCheckedBaggage,
                CheckedBaggagePriceMultiplier = route.CheckedBaggagePriceMultiplier,
                DepartureAirport = new RouteAirport
                {
                    Code = route.DepartureAirportCode,
                    Name = route.DepartureAirportName,
                    City = route.DepartureAirportCity
                },
                ArrivalAirport = new RouteAirport
                {
                    Code = route.ArrivalAirportCode,
                    Name = route.ArrivalAirportName,
                    City = route.ArrivalAirportCity
                }
            };
        }

        private static void AddDepartureWindowFilters(
            ICollection<string> where,
            DynamicParameters parameters,
            IReadOnlyCollection<RouteDepartureWindow> departureWindows)
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

        private static object ToParameters(DomainRoute route)
        {
            return new
            {
                route.Id,
                route.AirplaneId,
                route.DepartureAirportId,
                route.ArrivalAirportId,
                DepartureTime = route.DepartureTime.ToTimeSpan(),
                ArrivalTime = route.ArrivalTime.ToTimeSpan(),
                Frequency = route.Frequency.ToByte(),
                route.DurationMinutes,
                route.PriceFirstClass,
                route.PriceEconomyClass,
                route.PriceCarryOnBaggage,
                route.PriceCheckedBaggage,
                route.WeightLimitCarryOnBaggage,
                route.WeightLimitCheckedBaggage,
                route.CheckedBaggagePriceMultiplier
            };
        }

        private class RouteRecord
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

        private class RouteSearchRecord
        {
            public int Id { get; set; }
            public int AirplaneId { get; set; }
            public int DepartureAirportId { get; set; }
            public int ArrivalAirportId { get; set; }
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
            public decimal PriceEconomyClass { get; set; }
            public decimal PriceFirstClass { get; set; }
            public decimal PriceCarryOnBaggage { get; set; }
            public decimal PriceCheckedBaggage { get; set; }
            public int WeightLimitCarryOnBaggage { get; set; }
            public int WeightLimitCheckedBaggage { get; set; }
            public decimal CheckedBaggagePriceMultiplier { get; set; }
        }
    }
}
