CREATE OR ALTER FUNCTION dbo.GetPassengerCheckedLuggageTotalCost(
    @booking_id UNIQUEIDENTIFIER,
    @passenger_id INT
)
RETURNS DECIMAL(10, 2)
AS
BEGIN
    DECLARE @checked_luggage INT;
    DECLARE @bag_number INT = 1;
    DECLARE @total DECIMAL(18, 4) = 0;

    SELECT
        @checked_luggage = passenger.CheckedLuggage
    FROM dbo.booking booking
    INNER JOIN dbo.Passenger passenger
        ON passenger.PurchaseOrderId = booking.purchase_order_id
    WHERE booking.guid = @booking_id
      AND passenger.Id = @passenger_id;

    IF @checked_luggage IS NULL
    BEGIN
        RETURN NULL;
    END;

    IF @checked_luggage < 1
    BEGIN
        RETURN 0;
    END;

    WHILE @bag_number <= @checked_luggage
    BEGIN
        SELECT
            @total = @total + COALESCE(SUM(
                CAST(luggage_leg.CheckedPrice AS DECIMAL(18, 4))
                * CAST(
                    POWER(
                        1.0 + CAST(luggage_leg.CheckedMultiplier AS FLOAT),
                        @bag_number - 1
                    ) AS DECIMAL(18, 8)
                )
            ), 0)
        FROM (
            SELECT
                route.price_checked_baggage AS CheckedPrice,
                COALESCE(route.checked_baggage_price_multiplier, 0) AS CheckedMultiplier
            FROM dbo.booking booking
            INNER JOIN dbo.itinerary itinerary
                ON itinerary.booking_guid = booking.guid
            INNER JOIN dbo.flight_internal internal_flight
                ON internal_flight.flight_guid = itinerary.flight_guid
            INNER JOIN dbo.[route] route
                ON route.id = internal_flight.route_id
                AND route.is_deleted = 0
            WHERE booking.guid = @booking_id

            UNION ALL

            SELECT
                external_flight.checked_price AS CheckedPrice,
                CAST(0 AS DECIMAL(5, 4)) AS CheckedMultiplier
            FROM dbo.booking booking
            INNER JOIN dbo.itinerary itinerary
                ON itinerary.booking_guid = booking.guid
            INNER JOIN dbo.flight_external external_flight
                ON external_flight.flight_guid = itinerary.flight_guid
            WHERE booking.guid = @booking_id
        ) luggage_leg;

        SET @bag_number = @bag_number + 1;
    END;

    RETURN CAST(ROUND(@total, 2) AS DECIMAL(10, 2));
END;
GO
