-- Booking total now includes luggage:
--   total = flights + carry_on + checked
--   flights    = SUM(unit_price across legs) * passenger_count
--   carry_on   = SUM(passenger.CarryOnLuggage) * SUM(route.price_carry_on_baggage across legs)
--   checked    = SUM(passenger.CheckedLuggage) * SUM(route.price_checked_baggage across legs)

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
        @seat_class VARCHAR(50),
        @passenger_count INT,
        @total_carry_on_count INT,
        @total_checked_count INT,
        @total_amount DECIMAL(10, 2),
        @booking_guid UNIQUEIDENTIFIER,
        @confirmation_code VARCHAR(12),
        @route_count INT,
        @current_sequence INT,
        @current_route_id INT,
        @current_departure_at DATETIME2(0),
        @current_arrival_at DATETIME2(0),
        @current_arrival_airport_id INT,
        @current_departure_airport_id INT,
        @current_flight_guid UNIQUEIDENTIFIER,
        @current_remaining_seats INT,
        @previous_arrival_at DATETIME2(0),
        @previous_arrival_airport_id INT;

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
        @seat_class = SeatClass
    FROM dbo.PurchaseOrder WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @purchase_order_id;

    IF @seat_class IS NULL
    BEGIN
        THROW 50000, 'Purchase order was not found.', 1;
    END;

    DECLARE @legs TABLE (
        sequence_number       INT              NOT NULL PRIMARY KEY,
        route_id              INT              NOT NULL,
        intended_date         DATE             NULL,
        departure_airport_id  INT              NULL,
        arrival_airport_id    INT              NULL,
        departure_at          DATETIME2(0)     NULL,
        arrival_at            DATETIME2(0)     NULL,
        unit_price            DECIMAL(10, 2)   NULL,
        carry_on_price        DECIMAL(10, 2)   NULL,
        checked_price         DECIMAL(10, 2)   NULL,
        flight_guid           UNIQUEIDENTIFIER NULL
    );

    INSERT INTO
        @legs (sequence_number, route_id, intended_date)
    SELECT
        SequenceNumber, RouteId, IntendedDate
    FROM
        dbo.PurchaseOrderRoute WITH (UPDLOCK, HOLDLOCK)
    WHERE
        PurchaseOrderId = @purchase_order_id;

    SELECT @route_count = COUNT(*) FROM @legs;

    IF @route_count = 0
    BEGIN
        THROW 50000, 'Purchase order has no routes.', 1;
    END;

    IF EXISTS (SELECT 1 FROM @legs WHERE intended_date IS NULL)
    BEGIN
        THROW 50000, 'Purchase order intended date is required for every leg.', 1;
    END;

    IF EXISTS (
        SELECT
            1
        FROM
            dbo.booking WITH (UPDLOCK, HOLDLOCK)
        WHERE
            purchase_order_id = @purchase_order_id
    )
    BEGIN
        THROW 50000, 'Purchase order is already booked.', 1;
    END;

    SELECT
        @passenger_count = COUNT(*),
        @total_carry_on_count = COALESCE(SUM(CarryOnLuggage), 0),
        @total_checked_count = COALESCE(SUM(CheckedLuggage), 0)
    FROM dbo.Passenger
    WHERE PurchaseOrderId = @purchase_order_id;

    IF @passenger_count < 1
    BEGIN
        THROW 50000, 'Purchase order must have at least one passenger.', 1;
    END;

    UPDATE
        l
    SET departure_airport_id = r.departure_airport_id,
        arrival_airport_id = r.arrival_airport_id,
        unit_price = CASE @seat_class
            WHEN 'economy' THEN r.price_economy_class
            WHEN 'firstClass' THEN r.price_first_class
        END,
        carry_on_price = r.price_carry_on_baggage,
        checked_price = r.price_checked_baggage
    FROM
        @legs l
        INNER JOIN dbo.[route] r WITH (UPDLOCK, HOLDLOCK) ON r.id = l.route_id;

    IF EXISTS (SELECT 1 FROM @legs WHERE departure_airport_id IS NULL)
    BEGIN
        THROW 50000, 'Route was not found.', 1;
    END;

    IF EXISTS (SELECT 1 FROM @legs WHERE unit_price IS NULL)
    BEGIN
        THROW 50000, 'Unsupported seat class.', 1;
    END;

    UPDATE
        l
    SET departure_at = f.departure_at,
        arrival_at = f.arrival_at
    FROM
        @legs l
    CROSS APPLY
        dbo.route_flights_at(l.intended_date) f
    WHERE
        f.route_id = l.route_id;

    IF EXISTS (SELECT 1 FROM @legs WHERE departure_at IS NULL)
    BEGIN
        THROW 50000, 'Route does not operate on the requested departure date.', 1;
    END;

    SET @current_sequence = 1;
    SET @previous_arrival_at = NULL;
    SET @previous_arrival_airport_id = NULL;

    WHILE @current_sequence <= @route_count
    BEGIN
        SELECT
            @current_departure_airport_id = departure_airport_id,
            @current_arrival_airport_id = arrival_airport_id,
            @current_departure_at = departure_at,
            @current_arrival_at = arrival_at
        FROM
            @legs
        WHERE
            sequence_number = @current_sequence;

        IF @previous_arrival_airport_id IS NOT NULL
            AND @previous_arrival_airport_id <> @current_departure_airport_id
        BEGIN
            THROW 50000, 'Itinerary legs do not connect: arrival and departure airports must match.', 1;
        END;

        IF @previous_arrival_at IS NOT NULL
            AND @previous_arrival_at > @current_departure_at
        BEGIN
            THROW 50000, 'Itinerary legs do not connect: next leg departs before previous leg arrives.', 1;
        END;

        SET @previous_arrival_airport_id = @current_arrival_airport_id;
        SET @previous_arrival_at = @current_arrival_at;
        SET @current_sequence = @current_sequence + 1;
    END;

    -- Find or create each flight and validate remaining seats.
    DECLARE @created_flight TABLE (guid UNIQUEIDENTIFIER NOT NULL);

    SET @current_sequence = 1;

    WHILE @current_sequence <= @route_count
    BEGIN
        SELECT
            @current_route_id = route_id,
            @current_departure_at = departure_at,
            @current_arrival_at = arrival_at
        FROM @legs
        WHERE sequence_number = @current_sequence;

        SET @current_flight_guid = NULL;

        SELECT @current_flight_guid = guid
        FROM dbo.flight WITH (UPDLOCK, HOLDLOCK)
        WHERE route_id = @current_route_id
            AND departure_at = @current_departure_at;

        IF @current_flight_guid IS NULL
        BEGIN
            DELETE FROM @created_flight;

            INSERT INTO dbo.flight (
                route_id,
                departure_at,
                arrival_at
            )
            OUTPUT INSERTED.guid INTO @created_flight
            VALUES (
                @current_route_id,
                @current_departure_at,
                @current_arrival_at
            );

            SELECT @current_flight_guid = guid
            FROM @created_flight;
        END
        ELSE IF EXISTS (
            SELECT 1
            FROM dbo.flight
            WHERE guid = @current_flight_guid
                AND status <> 'scheduled'
        )
        BEGIN
            THROW 50000, 'Flight is not available for booking.', 1;
        END;

        SET @current_remaining_seats = dbo.remaining_seats(@current_flight_guid);

        IF @current_remaining_seats IS NULL
        BEGIN
            THROW 50000, 'Could not calculate remaining seats for the flight.', 1;
        END;

        IF @current_remaining_seats < @passenger_count
        BEGIN
            THROW 50000, 'Flight does not have enough remaining seats for all passengers.', 1;
        END;

        UPDATE @legs
        SET flight_guid = @current_flight_guid
        WHERE sequence_number = @current_sequence;

        SET @current_sequence = @current_sequence + 1;
    END;

    SELECT @total_amount =
        (SUM(unit_price) * @passenger_count)
        + (@total_carry_on_count * SUM(carry_on_price))
        + (@total_checked_count * SUM(checked_price))
    FROM @legs;

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
        @confirmation_code,
        LTRIM(RTRIM(@email)),
        @total_amount,
        LTRIM(RTRIM(@card_brand)),
        @card_last_four,
        LTRIM(RTRIM(@card_holder_name))
    );

    SELECT @booking_guid = guid
    FROM @created_booking;

    INSERT INTO dbo.itinerary (booking_guid, sequence_number, flight_guid)
    SELECT @booking_guid, sequence_number, flight_guid
    FROM @legs;

    COMMIT TRANSACTION;

    SELECT
        guid AS Guid,
        purchase_order_id AS PurchaseOrderId,
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
