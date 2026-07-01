using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<MonthlyRevenueFilterOptions> GetMonthlyRevenueFilterOptionsAsync(CancellationToken cancellationToken)
        {
            const string originsSql = """
                SELECT DISTINCT
                    options.Id,
                    options.Code,
                    options.Name
                FROM (
                    SELECT
                        origin.id AS Id,
                        origin.code AS Code,
                        origin.name AS Name
                    FROM dbo.[route] r
                    JOIN dbo.airport origin ON origin.id = r.departure_airport_id

                    UNION

                    SELECT
                        origin.id AS Id,
                        origin.code AS Code,
                        origin.name AS Name
                    FROM dbo.flight_external external_flight
                    JOIN dbo.airport origin ON origin.code = external_flight.departure_airport_code
                ) options
                ORDER BY options.Code;
                """;

            const string destinationsSql = """
                SELECT DISTINCT
                    options.Id,
                    options.Code,
                    options.Name
                FROM (
                    SELECT
                        destination.id AS Id,
                        destination.code AS Code,
                        destination.name AS Name
                    FROM dbo.[route] r
                    JOIN dbo.airport destination ON destination.id = r.arrival_airport_id

                    UNION

                    SELECT
                        destination.id AS Id,
                        destination.code AS Code,
                        destination.name AS Name
                    FROM dbo.flight_external external_flight
                    JOIN dbo.airport destination ON destination.code = external_flight.arrival_airport_code
                ) options
                ORDER BY options.Code;
                """;

            const string airlinesSql = """
                SELECT
                    options.Id,
                    options.Name
                FROM (
                    SELECT 0 AS Id, 'SnoopyAirlines' AS Name
                    UNION
                    SELECT DISTINCT
                        partner_airline.id AS Id,
                        partner_airline.name AS Name
                    FROM dbo.partner_airline partner_airline
                ) options
                ORDER BY options.Name;
                """;

            await using var connection = new SqlConnection(_connectionString);

            var origins = await connection.QueryAsync<MonthlyRevenueAirportFilterOption>(
                new CommandDefinition(originsSql, cancellationToken: cancellationToken));
            var destinations = await connection.QueryAsync<MonthlyRevenueAirportFilterOption>(
                new CommandDefinition(destinationsSql, cancellationToken: cancellationToken));
            var airlines = await connection.QueryAsync<MonthlyRevenueAirlineFilterOption>(
                new CommandDefinition(airlinesSql, cancellationToken: cancellationToken));

            return new MonthlyRevenueFilterOptions
            {
                Origins = origins.ToList(),
                Destinations = destinations.ToList(),
                Airlines = airlines.ToList(),
            };
        }

        public async Task<IReadOnlyCollection<MonthlyRevenueReportRow>> GetMonthlyRevenueBreakdownAsync(
            int? year,
            int? originAirportId,
            int? destinationAirportId,
            int? partnerAirlineId,
            CancellationToken cancellationToken)
        {
            var startOfYear = year.HasValue ? new DateTime(year.Value, 1, 1) : (DateTime?)null;
            var startOfNextYear = startOfYear?.AddYears(1);

            const string sql = """
                WITH FilteredBookings AS (
                    SELECT
                        b.guid AS BookingGuid,
                        b.purchase_order_id AS PurchaseOrderId,
                        MONTH(b.confirmed_at) AS MonthNumber
                    FROM dbo.booking b
                    WHERE b.status = 'confirmed'
                      AND (
                            @Year IS NULL
                            OR (b.confirmed_at >= @StartOfYear AND b.confirmed_at < @StartOfNextYear)
                          )
                      AND EXISTS (
                            SELECT 1
                            FROM dbo.itinerary i
                            JOIN dbo.flight f ON f.guid = i.flight_guid
                                                        LEFT JOIN dbo.flight_internal internal_flight ON internal_flight.flight_guid = f.guid
                                                        LEFT JOIN dbo.[route] r ON r.id = internal_flight.route_id
                                                        LEFT JOIN dbo.flight_external external_flight ON external_flight.flight_guid = f.guid
                                                        LEFT JOIN dbo.airport origin_filter ON origin_filter.id = @OriginAirportId
                                                        LEFT JOIN dbo.airport destination_filter ON destination_filter.id = @DestinationAirportId
                            WHERE i.booking_guid = b.guid
                                                            AND (
                                                                        @OriginAirportId IS NULL
                                                                        OR r.departure_airport_id = @OriginAirportId
                                                                        OR external_flight.departure_airport_code = origin_filter.code
                                                                    )
                                                            AND (
                                                                        @DestinationAirportId IS NULL
                                                                        OR r.arrival_airport_id = @DestinationAirportId
                                                                        OR external_flight.arrival_airport_code = destination_filter.code
                                                                    )
                                                            AND (
                                                                @PartnerAirlineId IS NULL
                                                                OR (@PartnerAirlineId = 0 AND internal_flight.flight_guid IS NOT NULL)
                                                                OR (@PartnerAirlineId > 0 AND external_flight.partner_airline_id = @PartnerAirlineId)
                                                                )
                        )
                ),
                PassengerStats AS (
                    SELECT
                        po.Id AS PurchaseOrderId,
                        po.SeatClass,
                        COUNT(*) AS PassengerCount,
                        COALESCE(SUM(p.CarryOnLuggage), 0) AS CarryOnLuggageCount
                    FROM dbo.PurchaseOrder po
                    JOIN dbo.Passenger p ON p.PurchaseOrderId = po.Id
                    JOIN (
                        SELECT DISTINCT PurchaseOrderId
                        FROM FilteredBookings
                    ) yb ON yb.PurchaseOrderId = po.Id
                    GROUP BY po.Id, po.SeatClass
                ),
                LegStats AS (
                    SELECT
                        yb.BookingGuid,
                        COUNT(*) AS LegCount,
                        COALESCE(SUM(
                            CASE po.SeatClass
                                WHEN 'economy' THEN COALESCE(r.price_economy_class, external_flight.tourist_price, 0)
                                WHEN 'firstClass' THEN COALESCE(r.price_first_class, external_flight.first_class_price, 0)
                                ELSE 0
                            END
                        ), 0) AS TicketUnitPriceTotal,
                        COALESCE(SUM(COALESCE(r.price_carry_on_baggage, external_flight.carry_on_price, 0)), 0) AS CarryOnUnitPriceTotal
                    FROM FilteredBookings yb
                    JOIN dbo.PurchaseOrder po ON po.Id = yb.PurchaseOrderId
                    JOIN dbo.itinerary i ON i.booking_guid = yb.BookingGuid
                    JOIN dbo.flight f ON f.guid = i.flight_guid
                    LEFT JOIN dbo.flight_internal internal_flight ON internal_flight.flight_guid = f.guid
                    LEFT JOIN dbo.[route] r ON r.id = internal_flight.route_id
                    LEFT JOIN dbo.flight_external external_flight ON external_flight.flight_guid = f.guid
                    GROUP BY yb.BookingGuid
                ),
                CheckedRevenue AS (
                    SELECT
                        yb.BookingGuid,
                        COALESCE(SUM(ISNULL(p.checkedLuggagePaid, 0)), 0) AS CheckedRevenue
                    FROM FilteredBookings yb
                    JOIN dbo.Passenger p ON p.PurchaseOrderId = yb.PurchaseOrderId
                    GROUP BY yb.BookingGuid
                ),
                BookingRevenue AS (
                    SELECT
                        yb.MonthNumber,
                        1 AS FlightCount,
                        CASE WHEN ps.SeatClass = 'firstClass' THEN ps.PassengerCount ELSE 0 END AS FirstClassPassengers,
                        CASE WHEN ps.SeatClass = 'economy' THEN ps.PassengerCount ELSE 0 END AS EconomyPassengers,
                        ps.PassengerCount AS TotalPassengers,
                        CAST(ls.TicketUnitPriceTotal * ps.PassengerCount AS DECIMAL(18,2)) AS TicketRevenue,
                        CAST((ls.CarryOnUnitPriceTotal * ps.CarryOnLuggageCount) + ISNULL(cr.CheckedRevenue, 0) AS DECIMAL(18,2)) AS LuggageRevenue,
                        CAST(
                            (ls.TicketUnitPriceTotal * ps.PassengerCount)
                            + (ls.CarryOnUnitPriceTotal * ps.CarryOnLuggageCount)
                            + ISNULL(cr.CheckedRevenue, 0)
                            AS DECIMAL(18,2)
                        ) AS TotalRevenue
                    FROM FilteredBookings yb
                    JOIN PassengerStats ps ON ps.PurchaseOrderId = yb.PurchaseOrderId
                    JOIN LegStats ls ON ls.BookingGuid = yb.BookingGuid
                    LEFT JOIN CheckedRevenue cr ON cr.BookingGuid = yb.BookingGuid
                ),
                MonthlyRevenue AS (
                    SELECT
                        MonthNumber,
                        SUM(FlightCount) AS FlightCount,
                        SUM(FirstClassPassengers) AS FirstClassPassengers,
                        SUM(EconomyPassengers) AS EconomyPassengers,
                        SUM(TotalPassengers) AS TotalPassengers,
                        SUM(TicketRevenue) AS TicketRevenue,
                        SUM(LuggageRevenue) AS LuggageRevenue,
                        SUM(TotalRevenue) AS TotalRevenue
                    FROM BookingRevenue
                    GROUP BY MonthNumber
                ),
                Months AS (
                    SELECT MonthNumber
                    FROM (VALUES
                        (1), (2), (3), (4), (5), (6),
                        (7), (8), (9), (10), (11), (12)
                    ) AS MonthSeed(MonthNumber)
                )
                SELECT
                    months.MonthNumber AS MonthNumber,
                    COALESCE(monthly.FlightCount, 0) AS FlightCount,
                    COALESCE(monthly.FirstClassPassengers, 0) AS FirstClassPassengers,
                    COALESCE(monthly.EconomyPassengers, 0) AS EconomyPassengers,
                    COALESCE(monthly.TotalPassengers, 0) AS TotalPassengers,
                    CAST(COALESCE(monthly.TicketRevenue, 0) AS DECIMAL(18,2)) AS TicketRevenue,
                    CAST(COALESCE(monthly.LuggageRevenue, 0) AS DECIMAL(18,2)) AS LuggageRevenue,
                    CAST(COALESCE(monthly.TotalRevenue, 0) AS DECIMAL(18,2)) AS TotalRevenue
                FROM Months months
                LEFT JOIN MonthlyRevenue monthly ON monthly.MonthNumber = months.MonthNumber
                ORDER BY months.MonthNumber;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var rows = await connection.QueryAsync<MonthlyRevenueReportRow>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Year = year,
                        StartOfYear = startOfYear,
                        StartOfNextYear = startOfNextYear,
                        OriginAirportId = originAirportId,
                        DestinationAirportId = destinationAirportId,
                        PartnerAirlineId = partnerAirlineId,
                    },
                    cancellationToken: cancellationToken));

            return rows.ToList();
        }

        public async Task<IReadOnlyCollection<AirlineDetailedReportRow>> GetAirlineDetailedReportAsync(
            string? origin,
            string? destination,
            string? seatClass,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string? airline,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    Fecha,
                    Origen,
                    Destino,
                    FlightCode,
                    PasajerosPrimeraClase,
                    PasajerosEconomia,
                    Aerolinea,
                    VentaPasajeros,
                    VentaEquipajes,
                    TotalVenta
                FROM dbo.GetAirlineDetailedReport(@Origin, @Destination, @SeatClass, @DateFrom, @DateTo, @AirlineName)
                ORDER BY Fecha DESC, Origen, Destino;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var rows = await connection.QueryAsync<AirlineDetailedReportRow>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Origin = origin,
                        Destination = destination,
                        SeatClass = seatClass,
                        DateFrom = dateFrom.HasValue ? dateFrom.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        DateTo = dateTo.HasValue ? dateTo.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        AirlineName = airline
                    },
                    cancellationToken: cancellationToken));

            return rows.ToList();
        }
    }
}
