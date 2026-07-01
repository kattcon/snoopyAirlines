CREATE OR ALTER FUNCTION dbo.GetPurchaseOrderAmountBreakdown(
    @purchase_order_id INT
)
RETURNS TABLE
AS
RETURN (
    WITH passenger_stats AS (
        SELECT
            COUNT(*) AS PassengerCount,
            COALESCE(SUM(TRY_CAST(CarryOnLuggage AS DECIMAL(18, 2))), 0) AS CarryOnLuggageCount
        FROM dbo.Passenger
        WHERE PurchaseOrderId = @purchase_order_id
    ),
    leg_stats AS (
        SELECT
            COALESCE(SUM(
                CASE LOWER(po.SeatClass)
                    WHEN 'economy' THEN COALESCE(route.price_economy_class, external_flight.tourist_price, 0)
                    WHEN 'firstclass' THEN COALESCE(route.price_first_class, external_flight.first_class_price, 0)
                    ELSE 0
                END
            ), 0) AS TicketUnitPriceTotal,
            COALESCE(SUM(COALESCE(route.price_carry_on_baggage, external_flight.carry_on_price, 0)), 0) AS CarryOnUnitPriceTotal
        FROM dbo.PurchaseOrder po
        JOIN dbo.purchaseOrder_flight purchase_order_flight
            ON purchase_order_flight.PurchaseOrderId = po.Id
        JOIN dbo.flight flight
            ON flight.guid = purchase_order_flight.FlightGuid
        LEFT JOIN dbo.flight_internal internal_flight
            ON internal_flight.flight_guid = flight.guid
        LEFT JOIN dbo.[route] route
            ON route.id = internal_flight.route_id
            AND route.is_deleted = 0
        LEFT JOIN dbo.flight_external external_flight
            ON external_flight.flight_guid = flight.guid
        WHERE po.Id = @purchase_order_id
    ),
    checked_luggage AS (
        SELECT
            COALESCE(SUM(ISNULL(passenger.checkedLuggagePaid, 0)), 0)
                AS CheckedLuggageTotalAmount
        FROM dbo.Passenger passenger
        WHERE passenger.PurchaseOrderId = @purchase_order_id
    ),
    totals AS (
        SELECT
            CAST(leg_stats.TicketUnitPriceTotal * passenger_stats.PassengerCount AS DECIMAL(18, 2))
                AS TicketTotalAmount,
            CAST(leg_stats.CarryOnUnitPriceTotal * passenger_stats.CarryOnLuggageCount AS DECIMAL(18, 2))
                AS CarryOnLuggageTotalAmount,
            CAST(checked_luggage.CheckedLuggageTotalAmount AS DECIMAL(18, 2))
                AS CheckedLuggageTotalAmount
        FROM passenger_stats
        CROSS JOIN leg_stats
        CROSS JOIN checked_luggage
    )
    SELECT
        TicketTotalAmount,
        CarryOnLuggageTotalAmount,
        CheckedLuggageTotalAmount,
        CAST(TicketTotalAmount + CarryOnLuggageTotalAmount + CheckedLuggageTotalAmount AS DECIMAL(18, 2))
            AS TotalAmount
    FROM totals
);
GO
