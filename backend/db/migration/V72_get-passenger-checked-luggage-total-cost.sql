DROP TRIGGER IF EXISTS dbo.trg_passenger_paid_checked_luggage;
GO

DROP TRIGGER IF EXISTS dbo.trg_passenger_checkedLuggagePaid;
GO

IF COL_LENGTH(N'dbo.Passenger', N'checkedLuggagePaid') IS NULL
BEGIN
    ALTER TABLE dbo.Passenger
        ADD checkedLuggagePaid DECIMAL(18, 2) NOT NULL
            CONSTRAINT df_passenger_checkedLuggagePaid DEFAULT 0;
END;
GO

DROP FUNCTION IF EXISTS dbo.GetPassengerCheckedLuggageTotalCost;
GO

CREATE OR ALTER FUNCTION dbo.GetNewPassengerCheckedLuggageCost(
    @purchase_order_id INT,
    @bag_number INT
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
    DECLARE @total DECIMAL(18, 4) = 0;

    IF @bag_number IS NULL OR @bag_number < 1
    BEGIN
        RETURN 0;
    END;

    SELECT
        @total = COALESCE(SUM(
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

    RETURN CAST(ROUND(@total, 2) AS DECIMAL(18, 2));
END;
GO

WITH passenger_bags AS (
    SELECT
        passenger.Id,
        passenger.PurchaseOrderId,
        bag_numbers.BagNumber
    FROM dbo.Passenger passenger
    CROSS APPLY (
        SELECT number_source.BagNumber
        FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY object_id) AS BagNumber
            FROM sys.all_objects
        ) number_source
        WHERE number_source.BagNumber <= CASE
            WHEN passenger.CheckedLuggage > 0 THEN passenger.CheckedLuggage
            ELSE 0
        END
    ) bag_numbers
),
passenger_paid_amounts AS (
    SELECT
        passenger_bags.Id,
        SUM(dbo.GetNewPassengerCheckedLuggageCost(
            passenger_bags.PurchaseOrderId,
            passenger_bags.BagNumber
        )) AS CheckedLuggagePaid
    FROM passenger_bags
    GROUP BY passenger_bags.Id
)
UPDATE passenger
SET checkedLuggagePaid = CAST(COALESCE(paid_amounts.CheckedLuggagePaid, 0) AS DECIMAL(18, 2))
FROM dbo.Passenger passenger
LEFT JOIN passenger_paid_amounts paid_amounts
    ON paid_amounts.Id = passenger.Id
WHERE passenger.checkedLuggagePaid = 0;

GO

CREATE OR ALTER TRIGGER dbo.trg_passenger_checkedLuggagePaid
ON dbo.Passenger
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(CheckedLuggage)
    BEGIN
        RETURN;
    END;

    DECLARE @passenger_id INT;
    DECLARE @purchase_order_id INT;
    DECLARE @previous_checked_luggage INT;
    DECLARE @new_checked_luggage INT;
    DECLARE @bag_number INT;
    DECLARE @added_amount DECIMAL(18, 2);

    DECLARE checked_luggage_increases CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            inserted.Id,
            inserted.PurchaseOrderId,
            COALESCE(deleted.CheckedLuggage, 0) AS PreviousCheckedLuggage,
            inserted.CheckedLuggage AS NewCheckedLuggage
        FROM inserted
        LEFT JOIN deleted
            ON deleted.Id = inserted.Id
        WHERE inserted.CheckedLuggage > COALESCE(deleted.CheckedLuggage, 0);

    OPEN checked_luggage_increases;

    FETCH NEXT FROM checked_luggage_increases
    INTO @passenger_id, @purchase_order_id, @previous_checked_luggage, @new_checked_luggage;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @bag_number = @previous_checked_luggage + 1;
        SET @added_amount = 0;

        WHILE @bag_number <= @new_checked_luggage
        BEGIN
            SET @added_amount =
                @added_amount
                + dbo.GetNewPassengerCheckedLuggageCost(@purchase_order_id, @bag_number);

            SET @bag_number = @bag_number + 1;
        END;

        UPDATE dbo.Passenger
        SET checkedLuggagePaid = checkedLuggagePaid + @added_amount
        WHERE Id = @passenger_id;

        FETCH NEXT FROM checked_luggage_increases
        INTO @passenger_id, @purchase_order_id, @previous_checked_luggage, @new_checked_luggage;
    END;

    CLOSE checked_luggage_increases;
    DEALLOCATE checked_luggage_increases;
END;
GO
