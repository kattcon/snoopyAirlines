CREATE OR ALTER FUNCTION dbo.remaining_seats (
    @flight_guid UNIQUEIDENTIFIER
)
RETURNS INT
AS
BEGIN
    DECLARE @capacity INT;
    DECLARE @booked_seats INT;

    SELECT
        @capacity =
            (airplane.tourist_rows * airplane.tourist_columns)
            + (airplane.firstClass_rows * airplane.firstClass_columns)
    FROM
        dbo.flight
        INNER JOIN dbo.flight_internal ON flight_internal.flight_guid = flight.guid
        INNER JOIN dbo.[route] ON [route].id = flight_internal.route_id
        INNER JOIN dbo.airplane ON airplane.id = [route].airplane_id
    WHERE
        flight.guid = @flight_guid
        AND [route].is_deleted = 0;

    IF @capacity IS NULL
    BEGIN
        RETURN NULL;
    END;

    SELECT
        @booked_seats = COUNT(Passenger.Id)
    FROM
        dbo.itinerary
        INNER JOIN dbo.booking ON booking.guid = itinerary.booking_guid
        INNER JOIN dbo.Passenger ON Passenger.PurchaseOrderId = booking.purchase_order_id
    WHERE
        itinerary.flight_guid = @flight_guid;

    RETURN @capacity - COALESCE(@booked_seats, 0);
END;
GO

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
        @current_departure_at DATETIME2(0),
        @current_arrival_at DATETIME2(0),
        @current_departure_airport_code CHAR(3),
        @current_arrival_airport_code CHAR(3),
        @current_flight_guid UNIQUEIDENTIFIER,
        @current_requires_seat_check BIT,
        @current_remaining_seats INT,
        @previous_arrival_at DATETIME2(0),
        @previous_arrival_airport_code CHAR(3),
        @flights_amount DECIMAL(18,2) = 0,
        @carry_on_amount DECIMAL(18,2) = 0,
        @checked_amount DECIMAL(18,2) = 0;

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
        flight_guid           UNIQUEIDENTIFIER NOT NULL,
        departure_airport_code CHAR(3)         NULL,
        arrival_airport_code   CHAR(3)         NULL,
        departure_at          DATETIME2(0)     NULL,
        arrival_at            DATETIME2(0)     NULL,
        unit_price            DECIMAL(10, 2)   NULL,
        carry_on_price        DECIMAL(10, 2)   NULL,
        checked_price         DECIMAL(10, 2)   NULL,
        checked_multiplier    DECIMAL(5, 4)    NULL,
        requires_seat_check   BIT              NOT NULL DEFAULT 0
    );

    INSERT INTO
        @legs (sequence_number, flight_guid)
    SELECT
        SequenceNumber, FlightGuid
    FROM
        dbo.purchaseOrder_flight WITH (UPDLOCK, HOLDLOCK)
    WHERE
        PurchaseOrderId = @purchase_order_id;

    SELECT @route_count = COUNT(*) FROM @legs;

    IF @route_count = 0
    BEGIN
        THROW 50000, 'Purchase order has no routes.', 1;
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
    SET departure_airport_code = departure_airport.code,
        arrival_airport_code = arrival_airport.code,
        departure_at = flight.departure_at,
        arrival_at = flight.arrival_at,
        unit_price = CASE @seat_class
            WHEN 'economy' THEN route.price_economy_class
            WHEN 'firstClass' THEN route.price_first_class
        END,
        carry_on_price = route.price_carry_on_baggage,
        checked_price = route.price_checked_baggage,
        checked_multiplier = route.checked_baggage_price_multiplier,
        requires_seat_check = 1
    FROM
        @legs l
        INNER JOIN dbo.flight flight WITH (UPDLOCK, HOLDLOCK)
            ON flight.guid = l.flight_guid
        INNER JOIN dbo.flight_internal internal_flight WITH (UPDLOCK, HOLDLOCK)
            ON internal_flight.flight_guid = flight.guid
        INNER JOIN dbo.[route] route WITH (UPDLOCK, HOLDLOCK)
            ON route.id = internal_flight.route_id
            AND route.is_deleted = 0
        INNER JOIN dbo.airport departure_airport
            ON departure_airport.id = route.departure_airport_id
        INNER JOIN dbo.airport arrival_airport
            ON arrival_airport.id = route.arrival_airport_id;

    UPDATE
        l
    SET departure_airport_code = external_flight.departure_airport_code,
        arrival_airport_code = external_flight.arrival_airport_code,
        departure_at = flight.departure_at,
        arrival_at = flight.arrival_at,
        unit_price = CASE @seat_class
            WHEN 'economy' THEN external_flight.tourist_price
            WHEN 'firstClass' THEN external_flight.first_class_price
        END,
        carry_on_price = external_flight.carry_on_price,
        checked_price = external_flight.checked_price,
        checked_multiplier = 0,
        requires_seat_check = 0
    FROM
        @legs l
        INNER JOIN dbo.flight flight WITH (UPDLOCK, HOLDLOCK)
            ON flight.guid = l.flight_guid
        INNER JOIN dbo.flight_external external_flight WITH (UPDLOCK, HOLDLOCK)
            ON external_flight.flight_guid = flight.guid;

    IF EXISTS (SELECT 1 FROM @legs WHERE departure_airport_code IS NULL)
    BEGIN
        THROW 50000, 'Purchase order contains a flight that is unavailable or has an unsupported type.', 1;
    END;

    IF EXISTS (SELECT 1 FROM @legs WHERE unit_price IS NULL)
    BEGIN
        THROW 50000, 'Unsupported seat class.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM @legs l
        INNER JOIN dbo.flight flight ON flight.guid = l.flight_guid
        WHERE flight.status <> 'scheduled'
    )
    BEGIN
        THROW 50000, 'Flight is not available for booking.', 1;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM @legs
        WHERE sequence_number = 1
          AND requires_seat_check = 1
    )
    BEGIN
        THROW 50000, 'The first itinerary leg must be operated by Snoopy Airlines.', 1;
    END;

    SET @current_sequence = 1;
    SET @previous_arrival_at = NULL;
    SET @previous_arrival_airport_code = NULL;

    WHILE @current_sequence <= @route_count
    BEGIN
        SELECT
            @current_departure_airport_code = departure_airport_code,
            @current_arrival_airport_code = arrival_airport_code,
            @current_departure_at = departure_at,
            @current_arrival_at = arrival_at
        FROM
            @legs
        WHERE
            sequence_number = @current_sequence;

        IF @previous_arrival_airport_code IS NOT NULL
            AND @previous_arrival_airport_code <> @current_departure_airport_code
        BEGIN
            THROW 50000, 'Itinerary legs do not connect: arrival and departure airports must match.', 1;
        END;

        IF @previous_arrival_at IS NOT NULL
            AND @previous_arrival_at > @current_departure_at
        BEGIN
            THROW 50000, 'Itinerary legs do not connect: next leg departs before previous leg arrives.', 1;
        END;

        SET @previous_arrival_airport_code = @current_arrival_airport_code;
        SET @previous_arrival_at = @current_arrival_at;
        SET @current_sequence = @current_sequence + 1;
    END;

    SET @current_sequence = 1;

    WHILE @current_sequence <= @route_count
    BEGIN
        SELECT
            @current_flight_guid = flight_guid,
            @current_requires_seat_check = requires_seat_check
        FROM @legs
        WHERE sequence_number = @current_sequence;

        IF @current_requires_seat_check = 0
        BEGIN
            SET @current_sequence = @current_sequence + 1;
            CONTINUE;
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

        SET @current_sequence = @current_sequence + 1;
    END;

    SELECT
        @flights_amount = COALESCE(SUM(unit_price), 0) * @passenger_count,
        @carry_on_amount = COALESCE(SUM(carry_on_price), 0) * @total_carry_on_count
    FROM @legs;

    SELECT
        @checked_amount = COALESCE(SUM(
            l.checked_price
            * (
                CAST(p.CheckedLuggage AS DECIMAL(18,4))
                + ISNULL(l.checked_multiplier, 0) * (CAST(p.CheckedLuggage AS DECIMAL(18,4)) * (CAST(p.CheckedLuggage AS DECIMAL(18,4)) - 1) / 2.0)
            )
        ), 0)
    FROM @legs l
    JOIN dbo.Passenger p ON p.PurchaseOrderId = @purchase_order_id
    WHERE p.CheckedLuggage > 0;

    SET @total_amount = @flights_amount + @carry_on_amount + @checked_amount;

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

CREATE OR ALTER FUNCTION dbo.GetBookingItineraryDetails(@bookingGuid UNIQUEIDENTIFIER)
RETURNS TABLE
AS
RETURN (
    WITH legs AS (
        SELECT
            b.guid                  AS BookingGuid,
            b.confirmation_code     AS ConfirmationCode,
            b.email                 AS Email,
            b.status                AS BookingStatus,
            b.total_amount          AS TotalAmount,
            b.card_brand            AS CardBrand,
            b.card_last_four        AS CardLastFour,
            b.card_holder_name      AS CardHolderName,
            b.created_at            AS CreatedAt,
            b.confirmed_at          AS ConfirmedAt,
            po.Id                   AS PurchaseOrderId,
            po.SeatClass,
            i.sequence_number       AS LegSequence,
            da.name                 AS DepartureAirportName,
            da.code                 AS DepartureAirportCode,
            dc.name                 AS DepartureCityName,
            aa.name                 AS ArrivalAirportName,
            aa.code                 AS ArrivalAirportCode,
            ac.name                 AS ArrivalCityName,
            f.departure_at          AS DepartureAt,
            f.arrival_at            AS ArrivalAt,
            f.status                AS FlightStatus,
            r.duration_minutes      AS LegDurationMinutes,
            ap.model                AS AirplaneModel
        FROM dbo.booking b
        JOIN dbo.PurchaseOrder po    ON po.Id                  = b.purchase_order_id
        JOIN dbo.itinerary i         ON i.booking_guid         = b.guid
        JOIN dbo.flight f            ON f.guid                 = i.flight_guid
        JOIN dbo.flight_internal fi  ON fi.flight_guid         = f.guid
        JOIN dbo.[route] r           ON r.id                   = fi.route_id
        JOIN dbo.airport da          ON da.id                  = r.departure_airport_id
        JOIN dbo.city dc             ON dc.id                  = da.city_id
        JOIN dbo.airport aa          ON aa.id                  = r.arrival_airport_id
        JOIN dbo.city ac             ON ac.id                  = aa.city_id
        JOIN dbo.airplane ap         ON ap.id                  = r.airplane_id
        WHERE b.guid = @bookingGuid

        UNION ALL

        SELECT
            b.guid                  AS BookingGuid,
            b.confirmation_code     AS ConfirmationCode,
            b.email                 AS Email,
            b.status                AS BookingStatus,
            b.total_amount          AS TotalAmount,
            b.card_brand            AS CardBrand,
            b.card_last_four        AS CardLastFour,
            b.card_holder_name      AS CardHolderName,
            b.created_at            AS CreatedAt,
            b.confirmed_at          AS ConfirmedAt,
            po.Id                   AS PurchaseOrderId,
            po.SeatClass,
            i.sequence_number       AS LegSequence,
            ef.departure_airport_name AS DepartureAirportName,
            ef.departure_airport_code AS DepartureAirportCode,
            ef.departure_city       AS DepartureCityName,
            ef.arrival_airport_name AS ArrivalAirportName,
            ef.arrival_airport_code AS ArrivalAirportCode,
            ef.arrival_city         AS ArrivalCityName,
            f.departure_at          AS DepartureAt,
            f.arrival_at            AS ArrivalAt,
            f.status                AS FlightStatus,
            DATEDIFF(MINUTE, f.departure_at, f.arrival_at) AS LegDurationMinutes,
            partner_airline.name    AS AirplaneModel
        FROM dbo.booking b
        JOIN dbo.PurchaseOrder po       ON po.Id                  = b.purchase_order_id
        JOIN dbo.itinerary i            ON i.booking_guid         = b.guid
        JOIN dbo.flight f               ON f.guid                 = i.flight_guid
        JOIN dbo.flight_external ef     ON ef.flight_guid         = f.guid
        JOIN dbo.partner_airline partner_airline
                                        ON partner_airline.id     = ef.partner_airline_id
        WHERE b.guid = @bookingGuid
    ),
    first_leg AS (
        SELECT * FROM legs WHERE LegSequence = 1
    ),
    last_leg AS (
        SELECT * FROM legs
        WHERE LegSequence = (SELECT MAX(LegSequence) FROM legs)
    )
    SELECT
        fl.BookingGuid,
        fl.ConfirmationCode,
        fl.Email,
        fl.BookingStatus,
        fl.TotalAmount,
        fl.CardBrand,
        fl.CardLastFour,
        fl.CardHolderName,
        fl.CreatedAt,
        fl.ConfirmedAt,
        fl.PurchaseOrderId,
        fl.SeatClass,

        fl.DepartureAirportName,
        fl.DepartureAirportCode,
        fl.DepartureCityName,
        fl.DepartureAt,

        ll.ArrivalAirportName,
        ll.ArrivalAirportCode,
        ll.ArrivalCityName,
        ll.ArrivalAt,

        DATEDIFF(MINUTE, fl.DepartureAt, ll.ArrivalAt) AS TotalItineraryMinutes,
        (SELECT SUM(LegDurationMinutes) FROM legs)      AS TotalFlightMinutes,
        (SELECT COUNT(*) - 1 FROM legs)                 AS LayoverCount

    FROM first_leg fl
    CROSS JOIN last_leg ll
);
GO