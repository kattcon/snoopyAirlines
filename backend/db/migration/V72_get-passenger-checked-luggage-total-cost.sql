IF COL_LENGTH(N'dbo.Passenger', N'paid_checked_luggage') IS NULL
BEGIN
    ALTER TABLE dbo.Passenger
        ADD paid_checked_luggage INT NOT NULL
            CONSTRAINT df_passenger_paid_checked_luggage DEFAULT 0;
END;
GO

UPDATE dbo.Passenger
SET paid_checked_luggage = CASE
    WHEN CheckedLuggage > 0 THEN CheckedLuggage
    ELSE 0
END
WHERE paid_checked_luggage < CASE
    WHEN CheckedLuggage > 0 THEN CheckedLuggage
    ELSE 0
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_passenger_paid_checked_luggage
ON dbo.Passenger
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(CheckedLuggage)
    BEGIN
        RETURN;
    END;

    WITH paid_luggage_adjustments AS (
        SELECT
            inserted.Id,
            COALESCE(deleted.paid_checked_luggage, passenger.paid_checked_luggage, 0)
                AS BasePaidCheckedLuggage,
            CASE
                WHEN inserted.CheckedLuggage > COALESCE(deleted.CheckedLuggage, 0)
                    THEN inserted.CheckedLuggage - COALESCE(deleted.CheckedLuggage, 0)
                ELSE 0
            END AS AddedCheckedLuggage
        FROM inserted
        LEFT JOIN deleted
            ON deleted.Id = inserted.Id
        JOIN dbo.Passenger passenger
            ON passenger.Id = inserted.Id
    )
    UPDATE passenger
    SET paid_checked_luggage =
        adjustments.BasePaidCheckedLuggage + adjustments.AddedCheckedLuggage
    FROM dbo.Passenger passenger
    JOIN paid_luggage_adjustments adjustments
        ON adjustments.Id = passenger.Id
    WHERE adjustments.AddedCheckedLuggage > 0
      AND passenger.paid_checked_luggage
            <> adjustments.BasePaidCheckedLuggage + adjustments.AddedCheckedLuggage;
END;
GO

CREATE OR ALTER FUNCTION dbo.GetPassengerCheckedLuggageTotalCost(
    @purchase_order_id INT,
    @passenger_id INT
)
RETURNS DECIMAL(10, 2)
AS
BEGIN
    DECLARE @paid_checked_luggage INT;
    DECLARE @bag_number INT = 1;
    DECLARE @total DECIMAL(18, 4) = 0;

    SELECT
        @paid_checked_luggage = passenger.paid_checked_luggage
    FROM dbo.Passenger passenger
    WHERE passenger.PurchaseOrderId = @purchase_order_id
      AND passenger.Id = @passenger_id;

    IF @paid_checked_luggage IS NULL
    BEGIN
        RETURN NULL;
    END;

    IF @paid_checked_luggage < 1
    BEGIN
        RETURN 0;
    END;

    WHILE @bag_number <= @paid_checked_luggage
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
            FROM dbo.purchaseOrder_flight purchase_order_flight
            INNER JOIN dbo.flight_internal internal_flight
                ON internal_flight.flight_guid = purchase_order_flight.FlightGuid
            INNER JOIN dbo.[route] route
                ON route.id = internal_flight.route_id
                AND route.is_deleted = 0
            WHERE purchase_order_flight.PurchaseOrderId = @purchase_order_id

            UNION ALL

            SELECT
                external_flight.checked_price AS CheckedPrice,
                CAST(0 AS DECIMAL(5, 4)) AS CheckedMultiplier
            FROM dbo.purchaseOrder_flight purchase_order_flight
            INNER JOIN dbo.flight_external external_flight
                ON external_flight.flight_guid = purchase_order_flight.FlightGuid
            WHERE purchase_order_flight.PurchaseOrderId = @purchase_order_id
        ) luggage_leg;

        SET @bag_number = @bag_number + 1;
    END;

    RETURN CAST(ROUND(@total, 2) AS DECIMAL(10, 2));
END;
GO
