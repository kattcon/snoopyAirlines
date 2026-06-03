CREATE OR ALTER PROCEDURE dbo.book
    @purchase_order_id INT,
    @email VARCHAR(254),
    @card_brand VARCHAR(30),
    @card_last_four CHAR(4),
    @card_holder_name VARCHAR(120)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    DECLARE
        @route_id INT,
        @seat_class VARCHAR(50),
        @intended_date DATE,
        @passenger_count INT,
        @unit_price DECIMAL(10, 2),
        @total_amount DECIMAL(10, 2),
        @departure_at DATETIME2(0),
        @arrival_at DATETIME2(0),
        @flight_guid UNIQUEIDENTIFIER,
        @booking_guid UNIQUEIDENTIFIER,
        @confirmation_code VARCHAR(12),
        @remaining_seats INT,
        @route_exists BIT;

    IF NULLIF(LTRIM(RTRIM(@email)), '') IS NULL
    BEGIN
        THROW 50000, 'email is required.', 1;
    END;

    IF NULLIF(LTRIM(RTRIM(@card_brand)), '') IS NULL
    BEGIN
        THROW 50000, 'card_brand is required.', 1;
    END;

    IF NULLIF(LTRIM(RTRIM(@card_last_four)), '') IS NULL
    BEGIN
        THROW 50000, 'card_last_four is required.', 1;
    END;

    IF NULLIF(LTRIM(RTRIM(@card_holder_name)), '') IS NULL
    BEGIN
        THROW 50000, 'card_holder_name is required.', 1;
    END;

    BEGIN TRANSACTION;

    SELECT
        @route_id = RouteId,
        @seat_class = SeatClass,
        @intended_date = IntendedDate
    FROM dbo.PurchaseOrder WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @purchase_order_id;

    IF @route_id IS NULL
    BEGIN
        THROW 50000, 'Purchase order was not found.', 1;
    END;

    IF @intended_date IS NULL
    BEGIN
        THROW 50000, 'Purchase order intended date is required.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.booking WITH (UPDLOCK, HOLDLOCK)
        WHERE purchase_order_id = @purchase_order_id
    )
    BEGIN
        THROW 50000, 'Purchase order is already booked.', 1;
    END;

    SET @route_exists = 0;

    SELECT
        @route_exists = 1,
        @unit_price = CASE @seat_class
            WHEN 'economy' THEN price_economy_class
            WHEN 'firstClass' THEN price_first_class
        END
    FROM dbo.[route] WITH (UPDLOCK, HOLDLOCK)
    WHERE id = @route_id;

    IF @route_exists = 0
    BEGIN
        THROW 50000, 'Route was not found.', 1;
    END;

    IF @unit_price IS NULL
    BEGIN
        THROW 50000, 'Unsupported seat class.', 1;
    END;

    SELECT @passenger_count = COUNT(*)
    FROM dbo.Passenger
    WHERE PurchaseOrderId = @purchase_order_id;

    IF @passenger_count < 1
    BEGIN
        THROW 50000, 'Purchase order must have at least one passenger.', 1;
    END;

    SELECT
        @departure_at = departure_at,
        @arrival_at = arrival_at
    FROM dbo.route_flights_at(@intended_date)
    WHERE route_id = @route_id;

    IF @departure_at IS NULL
    BEGIN
        THROW 50000, 'Route does not operate on the requested departure date.', 1;
    END;

    SELECT
        @flight_guid = guid
    FROM dbo.flight WITH (UPDLOCK, HOLDLOCK)
    WHERE route_id = @route_id
        AND departure_at = @departure_at;

    IF @flight_guid IS NULL
    BEGIN
        DECLARE @created_flight TABLE (guid UNIQUEIDENTIFIER NOT NULL);

        INSERT INTO dbo.flight (
            route_id,
            departure_at,
            arrival_at
        )
        OUTPUT INSERTED.guid INTO @created_flight
        VALUES (
            @route_id,
            @departure_at,
            @arrival_at
        );

        SELECT @flight_guid = guid
        FROM @created_flight;
    END;
    ELSE IF EXISTS (
        SELECT 1
        FROM dbo.flight
        WHERE guid = @flight_guid
            AND status <> 'scheduled'
    )
    BEGIN
        THROW 50000, 'Flight is not available for booking.', 1;
    END;

    SET @remaining_seats = dbo.remaining_seats(@flight_guid);

    IF @remaining_seats IS NULL
    BEGIN
        THROW 50000, 'Could not calculate remaining seats for the flight.', 1;
    END;

    IF @remaining_seats < @passenger_count
    BEGIN
        THROW 50000, 'Flight does not have enough remaining seats for all passengers.', 1;
    END;

    SET @total_amount = @unit_price * @passenger_count;

    WHILE @confirmation_code IS NULL
        OR EXISTS (
            SELECT 1
            FROM dbo.booking WITH (UPDLOCK, HOLDLOCK)
            WHERE confirmation_code = @confirmation_code
        )
    BEGIN
        SET @confirmation_code = UPPER(LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 12));
    END;

    DECLARE @created_booking TABLE (guid UNIQUEIDENTIFIER NOT NULL);

    INSERT INTO dbo.booking (
        purchase_order_id,
        flight_guid,
        confirmation_code,
        email,
        total_amount,
        card_brand,
        card_last_four,
        card_holder_name
    )
    OUTPUT INSERTED.guid INTO @created_booking
    VALUES (
        @purchase_order_id,
        @flight_guid,
        @confirmation_code,
        LTRIM(RTRIM(@email)),
        @total_amount,
        LTRIM(RTRIM(@card_brand)),
        @card_last_four,
        LTRIM(RTRIM(@card_holder_name))
    );

    SELECT @booking_guid = guid
    FROM @created_booking;

    COMMIT TRANSACTION;

    SELECT
        guid AS Guid,
        purchase_order_id AS PurchaseOrderId,
        flight_guid AS FlightGuid,
        confirmation_code AS ConfirmationCode,
        email AS Email,
        status AS Status,
        total_amount AS TotalAmount,
        card_brand AS CardBrand,
        card_last_four AS CardLastFour,
        card_holder_name AS CardHolderName,
        created_at AS CreatedAt,
        confirmed_at AS ConfirmedAt
    FROM dbo.booking
    WHERE guid = @booking_guid;
END;
GO
