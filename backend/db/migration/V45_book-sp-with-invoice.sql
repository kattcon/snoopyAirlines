-- El bloque de invoice se inserta después de dbo.itinerary y antes del COMMIT.

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
        @previous_arrival_airport_id INT,
        @flights_amount DECIMAL(18,2) = 0,
        @carry_on_amount DECIMAL(18,2) = 0,
        @checked_amount DECIMAL(18,2) = 0,
        @invoice_id INT,
        @total_carry_on_price DECIMAL(10,2);

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
        sequence_number INT NOT NULL PRIMARY KEY,
        route_id INT NOT NULL,
        intended_date DATE NULL,
        departure_airport_id INT NULL,
        arrival_airport_id INT NULL,
        departure_at DATETIME2(0) NULL,
        arrival_at DATETIME2(0) NULL,
        unit_price DECIMAL(10, 2) NULL,
        carry_on_price DECIMAL(10, 2) NULL,
        checked_price DECIMAL(10, 2) NULL,
        checked_multiplier DECIMAL(5, 4) NULL,
        flight_guid UNIQUEIDENTIFIER NULL
    );

    INSERT INTO @legs (sequence_number, route_id, intended_date)
    SELECT SequenceNumber, RouteId, IntendedDate
    FROM dbo.PurchaseOrderRoute WITH (UPDLOCK, HOLDLOCK)
    WHERE PurchaseOrderId = @purchase_order_id;

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
        SELECT 1
        FROM dbo.booking WITH (UPDLOCK, HOLDLOCK)
        WHERE purchase_order_id = @purchase_order_id
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

    UPDATE l
    SET departure_airport_id = r.departure_airport_id,
        arrival_airport_id = r.arrival_airport_id,
        unit_price = CASE @seat_class
            WHEN 'economy' THEN r.price_economy_class
            WHEN 'firstClass' THEN r.price_first_class
        END,
        carry_on_price = r.price_carry_on_baggage,
        checked_price = r.price_checked_baggage,
        checked_multiplier = r.checked_baggage_price_multiplier
    FROM @legs l
    INNER JOIN dbo.[route] r WITH (UPDLOCK, HOLDLOCK) ON r.id = l.route_id;

    IF EXISTS (SELECT 1 FROM @legs WHERE departure_airport_id IS NULL)
    BEGIN
        THROW 50000, 'Route was not found.', 1;
    END;

    IF EXISTS (SELECT 1 FROM @legs WHERE unit_price IS NULL)
    BEGIN
        THROW 50000, 'Unsupported seat class.', 1;
    END;

    UPDATE l
    SET departure_at = f.departure_at,
        arrival_at = f.arrival_at
    FROM @legs l
    CROSS APPLY dbo.route_flights_at(l.intended_date) f
    WHERE f.route_id = l.route_id;

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
        FROM @legs
        WHERE sequence_number = @current_sequence;

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

            INSERT INTO dbo.flight (route_id, departure_at, arrival_at)
            OUTPUT INSERTED.guid INTO @created_flight
            VALUES (@current_route_id, @current_departure_at, @current_arrival_at);

            SELECT @current_flight_guid = guid FROM @created_flight;
        END
        ELSE IF EXISTS (
            SELECT 1
            FROM dbo.flight
            WHERE guid = @current_flight_guid AND status <> 'scheduled'
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

    SELECT @booking_guid = guid FROM @created_booking;

    INSERT INTO dbo.itinerary (booking_guid, sequence_number, flight_guid)
    SELECT @booking_guid, sequence_number, flight_guid
    FROM @legs;

    -- -----------------------------------------------------------------------
    -- Facturación detallada
    -- -----------------------------------------------------------------------

    -- Cabecera: una factura por reserva
    DECLARE @created_invoice TABLE (id INT NOT NULL);

    INSERT INTO dbo.invoice (booking_guid)
    OUTPUT INSERTED.id INTO @created_invoice
    VALUES (@booking_guid);

    SELECT @invoice_id = id FROM @created_invoice;

    -- Costo de asiento: un registro por pasajero x tramo
    INSERT INTO dbo.invoice_flight (invoice_id, passenger_id, flight_guid, seat_class, unit_price)
    SELECT
        @invoice_id,
        p.Id,
        l.flight_guid,
        @seat_class,
        l.unit_price
    FROM dbo.Passenger p
    CROSS JOIN @legs l
    WHERE p.PurchaseOrderId = @purchase_order_id;

    -- Precio total de carry-on sumado a través de todos los tramos
    SELECT @total_carry_on_price = COALESCE(SUM(carry_on_price), 0) FROM @legs;

    -- Pre-calcular factores de checked baggage del viaje completo en escalares
    -- para evitar referencias cruzadas outer/inner en el SUM del INSERT.
    DECLARE @sum_checked_price DECIMAL(18, 4);
    DECLARE @sum_checked_multiplier DECIMAL(18, 4);
    DECLARE @sum_checked_price_x_multiplier DECIMAL(18, 4);

    SELECT
        @sum_checked_price              = COALESCE(SUM(checked_price), 0),
        @sum_checked_multiplier         = COALESCE(SUM(checked_multiplier), 0),
        @sum_checked_price_x_multiplier = COALESCE(SUM(checked_price * checked_multiplier), 0)
    FROM @legs;

    -- Costo de equipaje: un registro por pasajero para el viaje completo.
    -- checked_total = qty * sum_price + qty*(qty-1)/2 * sum_price_x_multiplier
    -- (equivalente matemático a la fórmula del SP dbo.book por pierna)
    INSERT INTO dbo.invoice_luggage (
        invoice_id,
        passenger_id,
        carry_on_quantity,
        carry_on_unit_price,
        carry_on_total,
        checked_quantity,
        checked_unit_price,
        checked_multiplier,
        checked_total
    )
    SELECT
        @invoice_id,
        p.Id,
        p.CarryOnLuggage,
        @total_carry_on_price,
        p.CarryOnLuggage * @total_carry_on_price,
        p.CheckedLuggage,
        @sum_checked_price,
        @sum_checked_multiplier,
        CAST(p.CheckedLuggage AS DECIMAL(18,4)) * @sum_checked_price
        + (CAST(p.CheckedLuggage AS DECIMAL(18,4)) * (CAST(p.CheckedLuggage AS DECIMAL(18,4)) - 1) / 2.0) * @sum_checked_price_x_multiplier
    FROM dbo.Passenger p
    WHERE p.PurchaseOrderId = @purchase_order_id;

    -- -----------------------------------------------------------------------

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
