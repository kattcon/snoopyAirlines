using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Airlines;
using SnoopyAirlines.Domain.View;
using System.Data;
using AirlineRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly string _connectionString;

        public FlightRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<Flight?> GetByGuidAsync(
            Guid flightGuid,
            CancellationToken cancellationToken)
        {
            var flights = await GetByGuidsAsync([flightGuid], cancellationToken);
            return flights.SingleOrDefault();
        }

        public async Task<IReadOnlyCollection<Flight>> GetByGuidsAsync(
            IReadOnlyCollection<Guid> flightGuids,
            CancellationToken cancellationToken)
        {
            if (flightGuids.Count == 0)
            {
                return Array.Empty<Flight>();
            }

            await using var connection = new SqlConnection(_connectionString);
            var records = await connection.QueryAsync<FlightRecord>(
                new CommandDefinition(
                    """
                    SELECT
                        flight.guid AS Guid,
                        flight.departure_at AS DepartureAt,
                        flight.arrival_at AS ArrivalAt,
                        flight.status AS Status,
                        flight.created_at AS CreatedAt,

                        internal_flight.route_id AS RouteId,

                        route.airplane_id AS AirplaneId,
                        route.departure_airport_id AS DepartureAirportId,
                        route.arrival_airport_id AS ArrivalAirportId,
                        route.departure_time AS RouteDepartureTime,
                        route.arrival_time AS RouteArrivalTime,
                        route.frequency AS RouteFrequency,
                        route.duration_minutes AS DurationMinutes,
                        route.price_first_class AS RoutePriceFirstClass,
                        route.price_economy_class AS RoutePriceEconomyClass,
                        route.price_carry_on_baggage AS RoutePriceCarryOnBaggage,
                        route.price_checked_baggage AS RoutePriceCheckedBaggage,
                        route.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                        route.weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                        route.checked_baggage_price_multiplier AS CheckedBaggagePriceMultiplier,
                        route_departure_airport.code AS RouteDepartureAirportCode,
                        route_departure_airport.name AS RouteDepartureAirportName,
                        route_departure_city.name AS RouteDepartureAirportCity,
                        route_arrival_airport.code AS RouteArrivalAirportCode,
                        route_arrival_airport.name AS RouteArrivalAirportName,
                        route_arrival_city.name AS RouteArrivalAirportCity,

                        external_flight.partner_airline_id AS PartnerAirlineId,
                        partner_airline.name AS PartnerAirlineName,
                        partner_airline.host AS PartnerAirlineHost,
                        partner_airline.api_key AS PartnerAirlineApiKey,
                        external_flight.departure_airport_code AS ExternalDepartureAirportCode,
                        external_flight.departure_airport_name AS ExternalDepartureAirportName,
                        external_flight.departure_city AS ExternalDepartureCity,
                        external_flight.arrival_airport_code AS ExternalArrivalAirportCode,
                        external_flight.arrival_airport_name AS ExternalArrivalAirportName,
                        external_flight.arrival_city AS ExternalArrivalCity,
                        external_flight.tourist_price AS ExternalTouristPrice,
                        external_flight.first_class_price AS ExternalFirstClassPrice,
                        external_flight.carry_on_price AS ExternalCarryOnPrice,
                        external_flight.checked_price AS ExternalCheckedPrice
                    FROM dbo.flight flight
                    LEFT JOIN dbo.flight_internal internal_flight
                        ON internal_flight.flight_guid = flight.guid
                    LEFT JOIN dbo.[route] route
                        ON route.id = internal_flight.route_id
                    LEFT JOIN dbo.airport route_departure_airport
                        ON route_departure_airport.id = route.departure_airport_id
                    LEFT JOIN dbo.city route_departure_city
                        ON route_departure_city.id = route_departure_airport.city_id
                    LEFT JOIN dbo.airport route_arrival_airport
                        ON route_arrival_airport.id = route.arrival_airport_id
                    LEFT JOIN dbo.city route_arrival_city
                        ON route_arrival_city.id = route_arrival_airport.city_id
                    LEFT JOIN dbo.flight_external external_flight
                        ON external_flight.flight_guid = flight.guid
                    LEFT JOIN dbo.partner_airline partner_airline
                        ON partner_airline.id = external_flight.partner_airline_id
                    WHERE flight.guid IN @FlightGuids
                    ORDER BY flight.departure_at, flight.guid;
                    """,
                    new { FlightGuids = flightGuids },
                    cancellationToken: cancellationToken));

            return records.Select(ToFlight).ToList();
        }

        public async Task<Guid> MaterializeInternalFlightAsync(
            int routeId,
            DateTime departureAt,
            CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    "dbo.materializeFlight",
                    new
                    {
                        RouteId = routeId,
                        DepartureAt = departureAt
                    },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }

        public async Task<Guid> MaterializeExternalFlightAsync(
            ExternalFlight flight,
            CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    """
                    SET XACT_ABORT ON;
                    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

                    BEGIN TRY
                        IF @FlightGuid IS NULL OR @FlightGuid = '00000000-0000-0000-0000-000000000000'
                        BEGIN
                            THROW 50000, 'FlightGuid is required for an external flight.', 1;
                        END;

                        BEGIN TRANSACTION;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM dbo.flight flight WITH (UPDLOCK, HOLDLOCK)
                            WHERE flight.guid = @FlightGuid
                        )
                        BEGIN
                            INSERT INTO dbo.flight (
                                guid,
                                departure_at,
                                arrival_at
                            )
                            VALUES (
                                @FlightGuid,
                                @DepartureAt,
                                @ArrivalAt
                            );

                            INSERT INTO dbo.flight_external (
                                flight_guid,
                                partner_airline_id,
                                departure_airport_code,
                                departure_airport_name,
                                departure_city,
                                arrival_airport_code,
                                arrival_airport_name,
                                arrival_city,
                                tourist_price,
                                first_class_price,
                                carry_on_price,
                                checked_price
                            )
                            VALUES (
                                @FlightGuid,
                                @PartnerAirlineId,
                                @DepartureAirportCode,
                                @DepartureAirportName,
                                @DepartureCity,
                                @ArrivalAirportCode,
                                @ArrivalAirportName,
                                @ArrivalCity,
                                @TouristPrice,
                                @FirstClassPrice,
                                @CarryOnPrice,
                                @CheckedPrice
                            );
                        END
                        ELSE IF NOT EXISTS (
                            SELECT 1
                            FROM dbo.flight_external external_flight WITH (UPDLOCK, HOLDLOCK)
                            WHERE external_flight.flight_guid = @FlightGuid
                              AND external_flight.partner_airline_id = @PartnerAirlineId
                        )
                        BEGIN
                            THROW 50000, 'External flight guid already exists for another flight owner.', 1;
                        END;

                        COMMIT TRANSACTION;

                        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

                        SELECT @FlightGuid;
                    END TRY
                    BEGIN CATCH
                        IF XACT_STATE() <> 0
                        BEGIN
                            ROLLBACK TRANSACTION;
                        END;

                        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

                        THROW;
                    END CATCH;
                    """,
                    new
                    {
                        FlightGuid = flight.Guid,
                        flight.PartnerAirlineId,
                        flight.DepartureAt,
                        flight.ArrivalAt,
                        DepartureAirportCode = flight.DepartureAirport.Code,
                        DepartureAirportName = flight.DepartureAirport.Name,
                        DepartureCity = flight.DepartureAirport.City,
                        ArrivalAirportCode = flight.ArrivalAirport.Code,
                        ArrivalAirportName = flight.ArrivalAirport.Name,
                        ArrivalCity = flight.ArrivalAirport.City,
                        flight.TouristPrice,
                        flight.FirstClassPrice,
                        flight.CarryOnPrice,
                        flight.CheckedPrice
                    },
                    cancellationToken: cancellationToken));
        }

        public async Task<FlightSearchResult> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = new CommandDefinition(
                """
                SELECT * FROM dbo.GetFlightReportByConfirmation(@ConfirmationNumber, @LastNames)
                ORDER BY SequenceNumber;
                """,
                new { ConfirmationNumber = confirmationNumber, LastNames = lastNames },
                cancellationToken: cancellationToken);

            var flightReport = await connection.QueryAsync<FlightReportView>(command);
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
                Legs = flightReport.ToList(),
                Passengers = passengers.ToList()
            };
        }

        private static Flight ToFlight(FlightRecord record)
        {
            if (record.RouteId is not null)
            {
                return new InternalFlight
                {
                    Guid = record.Guid,
                    DepartureAt = record.DepartureAt,
                    ArrivalAt = record.ArrivalAt,
                    Status = record.Status,
                    CreatedAt = record.CreatedAt,
                    RouteId = record.RouteId.Value,
                    Route = CreateRoute(record)
                };
            }

            if (record.PartnerAirlineId is not null)
            {
                return new ExternalFlight
                {
                    Guid = record.Guid,
                    DepartureAt = record.DepartureAt,
                    ArrivalAt = record.ArrivalAt,
                    Status = record.Status,
                    CreatedAt = record.CreatedAt,
                    PartnerAirlineId = record.PartnerAirlineId.Value,
                    PartnerAirline = new PartnerAirline
                    {
                        Id = record.PartnerAirlineId.Value,
                        Name = Required(record.PartnerAirlineName, nameof(record.PartnerAirlineName)),
                        Host = Required(record.PartnerAirlineHost, nameof(record.PartnerAirlineHost)),
                        ApiKey = Required(record.PartnerAirlineApiKey, nameof(record.PartnerAirlineApiKey))
                    },
                    DepartureAirport = new FlightAirport
                    {
                        Code = Required(record.ExternalDepartureAirportCode, nameof(record.ExternalDepartureAirportCode)),
                        Name = Required(record.ExternalDepartureAirportName, nameof(record.ExternalDepartureAirportName)),
                        City = Required(record.ExternalDepartureCity, nameof(record.ExternalDepartureCity))
                    },
                    ArrivalAirport = new FlightAirport
                    {
                        Code = Required(record.ExternalArrivalAirportCode, nameof(record.ExternalArrivalAirportCode)),
                        Name = Required(record.ExternalArrivalAirportName, nameof(record.ExternalArrivalAirportName)),
                        City = Required(record.ExternalArrivalCity, nameof(record.ExternalArrivalCity))
                    },
                    TouristPrice = Required(record.ExternalTouristPrice, nameof(record.ExternalTouristPrice)),
                    FirstClassPrice = Required(record.ExternalFirstClassPrice, nameof(record.ExternalFirstClassPrice)),
                    CarryOnPrice = Required(record.ExternalCarryOnPrice, nameof(record.ExternalCarryOnPrice)),
                    CheckedPrice = Required(record.ExternalCheckedPrice, nameof(record.ExternalCheckedPrice))
                };
            }

            throw new InvalidOperationException(
                $"Flight {record.Guid} is neither internal nor external.");
        }

        private static AirlineRoute CreateRoute(FlightRecord record)
        {
            return new AirlineRoute
            {
                Id = Required(record.RouteId, nameof(record.RouteId)),
                AirplaneId = Required(record.AirplaneId, nameof(record.AirplaneId)),
                DepartureAirportId = Required(record.DepartureAirportId, nameof(record.DepartureAirportId)),
                ArrivalAirportId = Required(record.ArrivalAirportId, nameof(record.ArrivalAirportId)),
                DepartureTime = TimeOnly.FromTimeSpan(Required(record.RouteDepartureTime, nameof(record.RouteDepartureTime))),
                ArrivalTime = TimeOnly.FromTimeSpan(Required(record.RouteArrivalTime, nameof(record.RouteArrivalTime))),
                Frequency = RouteFrequency.FromByte(Required(record.RouteFrequency, nameof(record.RouteFrequency))),
                DurationMinutes = Required(record.DurationMinutes, nameof(record.DurationMinutes)),
                PriceFirstClass = Required(record.RoutePriceFirstClass, nameof(record.RoutePriceFirstClass)),
                PriceEconomyClass = Required(record.RoutePriceEconomyClass, nameof(record.RoutePriceEconomyClass)),
                PriceCarryOnBaggage = Required(record.RoutePriceCarryOnBaggage, nameof(record.RoutePriceCarryOnBaggage)),
                PriceCheckedBaggage = Required(record.RoutePriceCheckedBaggage, nameof(record.RoutePriceCheckedBaggage)),
                WeightLimitCarryOnBaggage = Required(record.WeightLimitCarryOnBaggage, nameof(record.WeightLimitCarryOnBaggage)),
                WeightLimitCheckedBaggage = Required(record.WeightLimitCheckedBaggage, nameof(record.WeightLimitCheckedBaggage)),
                CheckedBaggagePriceMultiplier = Required(
                    record.CheckedBaggagePriceMultiplier,
                    nameof(record.CheckedBaggagePriceMultiplier)),
                DepartureAirport = new RouteAirport
                {
                    Code = Required(record.RouteDepartureAirportCode, nameof(record.RouteDepartureAirportCode)),
                    Name = Required(record.RouteDepartureAirportName, nameof(record.RouteDepartureAirportName)),
                    City = Required(record.RouteDepartureAirportCity, nameof(record.RouteDepartureAirportCity))
                },
                ArrivalAirport = new RouteAirport
                {
                    Code = Required(record.RouteArrivalAirportCode, nameof(record.RouteArrivalAirportCode)),
                    Name = Required(record.RouteArrivalAirportName, nameof(record.RouteArrivalAirportName)),
                    City = Required(record.RouteArrivalAirportCity, nameof(record.RouteArrivalAirportCity))
                }
            };
        }

        private static T Required<T>(T? value, string fieldName)
            where T : struct
        {
            return value ?? throw new InvalidOperationException(
                $"Flight record is missing required field {fieldName}.");
        }

        private static string Required(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Flight record is missing required field {fieldName}.");
            }

            return value;
        }

        private class FlightRecord
        {
            public Guid Guid { get; set; }
            public DateTime DepartureAt { get; set; }
            public DateTime ArrivalAt { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }

            public int? RouteId { get; set; }
            public int? AirplaneId { get; set; }
            public int? DepartureAirportId { get; set; }
            public int? ArrivalAirportId { get; set; }
            public TimeSpan? RouteDepartureTime { get; set; }
            public TimeSpan? RouteArrivalTime { get; set; }
            public byte? RouteFrequency { get; set; }
            public int? DurationMinutes { get; set; }
            public decimal? RoutePriceFirstClass { get; set; }
            public decimal? RoutePriceEconomyClass { get; set; }
            public decimal? RoutePriceCarryOnBaggage { get; set; }
            public decimal? RoutePriceCheckedBaggage { get; set; }
            public int? WeightLimitCarryOnBaggage { get; set; }
            public int? WeightLimitCheckedBaggage { get; set; }
            public decimal? CheckedBaggagePriceMultiplier { get; set; }
            public string? RouteDepartureAirportCode { get; set; }
            public string? RouteDepartureAirportName { get; set; }
            public string? RouteDepartureAirportCity { get; set; }
            public string? RouteArrivalAirportCode { get; set; }
            public string? RouteArrivalAirportName { get; set; }
            public string? RouteArrivalAirportCity { get; set; }

            public int? PartnerAirlineId { get; set; }
            public string? PartnerAirlineName { get; set; }
            public string? PartnerAirlineHost { get; set; }
            public string? PartnerAirlineApiKey { get; set; }
            public string? ExternalDepartureAirportCode { get; set; }
            public string? ExternalDepartureAirportName { get; set; }
            public string? ExternalDepartureCity { get; set; }
            public string? ExternalArrivalAirportCode { get; set; }
            public string? ExternalArrivalAirportName { get; set; }
            public string? ExternalArrivalCity { get; set; }
            public decimal? ExternalTouristPrice { get; set; }
            public decimal? ExternalFirstClassPrice { get; set; }
            public decimal? ExternalCarryOnPrice { get; set; }
            public decimal? ExternalCheckedPrice { get; set; }
        }
    }
}
