using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;

namespace snoopy_airlines_backend.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<MonthlyRevenueReportRow>> GetMonthlyRevenueBreakdownAsync(
            int year,
            CancellationToken cancellationToken)
        {
            var startOfYear = new DateTime(year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);

            const string sql = """
                WITH YearBookings AS (
                    SELECT
                        b.guid              AS BookingGuid,
                        b.purchase_order_id AS PurchaseOrderId,
                        MONTH(b.confirmed_at) AS MonthNumber
                    FROM dbo.booking b
                    WHERE b.status = 'confirmed'
                      AND b.confirmed_at >= @StartOfYear
                      AND b.confirmed_at < @StartOfNextYear
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
                        FROM YearBookings
                    ) yb ON yb.PurchaseOrderId = po.Id
                    GROUP BY po.Id, po.SeatClass
                ),
                LegStats AS (
                    SELECT
                        yb.BookingGuid,
                        COUNT(*) AS FlightCount,
                        COALESCE(SUM(
                            CASE po.SeatClass
                                WHEN 'economy' THEN r.price_economy_class
                                WHEN 'firstClass' THEN r.price_first_class
                                ELSE 0
                            END
                        ), 0) AS TicketUnitPriceTotal,
                        COALESCE(SUM(r.price_carry_on_baggage), 0) AS CarryOnUnitPriceTotal
                    FROM YearBookings yb
                    JOIN dbo.PurchaseOrder po ON po.Id = yb.PurchaseOrderId
                    JOIN dbo.itinerary i ON i.booking_guid = yb.BookingGuid
                    JOIN dbo.flight f ON f.guid = i.flight_guid
                    JOIN dbo.[route] r ON r.id = f.route_id
                    GROUP BY yb.BookingGuid
                ),
                CheckedRevenue AS (
                    SELECT
                        yb.BookingGuid,
                        COALESCE(SUM(
                            r.price_checked_baggage * (
                                CAST(p.CheckedLuggage AS DECIMAL(18,4))
                                + ISNULL(r.checked_baggage_price_multiplier, 0) * (
                                    CAST(p.CheckedLuggage AS DECIMAL(18,4))
                                    * (CAST(p.CheckedLuggage AS DECIMAL(18,4)) - 1)
                                    / 2.0
                                )
                            )
                        ), 0) AS CheckedRevenue
                    FROM YearBookings yb
                    JOIN dbo.itinerary i ON i.booking_guid = yb.BookingGuid
                    JOIN dbo.flight f ON f.guid = i.flight_guid
                    JOIN dbo.[route] r ON r.id = f.route_id
                    JOIN dbo.Passenger p ON p.PurchaseOrderId = yb.PurchaseOrderId
                    WHERE p.CheckedLuggage > 0
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
                    FROM YearBookings yb
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
                    new { StartOfYear = startOfYear, StartOfNextYear = startOfNextYear },
                    cancellationToken: cancellationToken));

            return rows.ToList();
        }
    }
}