SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

DECLARE @SeedOrders TABLE (
    OrderKey        VARCHAR(40)  NOT NULL PRIMARY KEY,
    SeatClass       VARCHAR(50)  NOT NULL,
    Email           VARCHAR(254) NOT NULL,
    ConfirmedAt     DATETIME2(0) NOT NULL,
    CardBrand       VARCHAR(30)  NOT NULL,
    CardLastFour    CHAR(4)      NOT NULL,
    CardHolderName  VARCHAR(120) NOT NULL
);

DECLARE @SeedPassengers TABLE (
    OrderKey          VARCHAR(40)  NOT NULL,
    SequenceNumber    INT          NOT NULL,
    Gender            VARCHAR(20)  NOT NULL,
    FirstName         VARCHAR(100) NOT NULL,
    LastName          VARCHAR(100) NOT NULL,
    BirthDay          VARCHAR(2)   NOT NULL,
    BirthMonth        VARCHAR(10)  NOT NULL,
    BirthYear         VARCHAR(4)   NOT NULL,
    Nationality       VARCHAR(100) NOT NULL,
    CarryOnLuggage    INT          NOT NULL,
    CheckedLuggage    INT          NOT NULL,
    PRIMARY KEY (OrderKey, SequenceNumber)
);

DECLARE @SeedLegs TABLE (
    OrderKey            VARCHAR(40)  NOT NULL,
    SequenceNumber      INT          NOT NULL,
    DepartureCode       CHAR(3)      NOT NULL,
    ArrivalCode         CHAR(3)      NOT NULL,
    DepartureAt         DATETIME2(0) NOT NULL,
    ArrivalAt           DATETIME2(0) NOT NULL,
    PRIMARY KEY (OrderKey, SequenceNumber)
);

INSERT INTO @SeedOrders (OrderKey, SeatClass, Email, ConfirmedAt, CardBrand, CardLastFour, CardHolderName)
VALUES
    ('REV-2026-01', 'economy',    'report-seed+2026-01@snoopy.local', '2026-01-17T14:05:00', 'VISA', '1101', 'Revenue Seed Jan'),
    ('REV-2026-02', 'firstClass', 'report-seed+2026-02@snoopy.local', '2026-02-06T16:40:00', 'MC',   '2202', 'Revenue Seed Feb'),
    ('REV-2026-03', 'economy',    'report-seed+2026-03@snoopy.local', '2026-03-21T09:15:00', 'AMEX', '3303', 'Revenue Seed Mar'),
    ('REV-2026-04', 'firstClass', 'report-seed+2026-04@snoopy.local', '2026-04-09T11:10:00', 'VISA', '4405', 'Revenue Seed Apr'),
    ('REV-2026-05', 'firstClass', 'report-seed+2026-05@snoopy.local', '2026-05-11T12:20:00', 'VISA', '4404', 'Revenue Seed May'),
    ('REV-2026-06', 'economy',    'report-seed+2026-06@snoopy.local', '2026-06-18T19:25:00', 'MC',   '5506', 'Revenue Seed Jun'),
    ('REV-2026-07', 'economy',    'report-seed+2026-07@snoopy.local', '2026-07-03T18:50:00', 'MC',   '5505', 'Revenue Seed Jul'),
    ('REV-2026-08', 'firstClass', 'report-seed+2026-08@snoopy.local', '2026-08-14T10:30:00', 'VISA', '6608', 'Revenue Seed Aug'),
    ('REV-2026-09', 'economy',    'report-seed+2026-09@snoopy.local', '2026-09-02T13:45:00', 'AMEX', '7709', 'Revenue Seed Sep'),
    ('REV-2026-10', 'firstClass', 'report-seed+2026-10@snoopy.local', '2026-10-19T15:05:00', 'MC',   '8810', 'Revenue Seed Oct'),
    ('REV-2026-11', 'economy',    'report-seed+2026-11@snoopy.local', '2026-11-08T08:55:00', 'VISA', '9911', 'Revenue Seed Nov'),
    ('REV-2026-12', 'firstClass', 'report-seed+2026-12@snoopy.local', '2026-12-22T17:35:00', 'AMEX', '1012', 'Revenue Seed Dec'),
    ('REV-2026-13', 'economy',    'report-seed+2026-13@snoopy.local', '2026-01-28T10:15:00', 'VISA', '1113', 'Revenue Seed Jan 2'),
    ('REV-2026-14', 'firstClass', 'report-seed+2026-14@snoopy.local', '2026-02-18T21:10:00', 'MC',   '1214', 'Revenue Seed Feb 2'),
    ('REV-2026-15', 'economy',    'report-seed+2026-15@snoopy.local', '2026-03-30T07:50:00', 'AMEX', '1315', 'Revenue Seed Mar 2'),
    ('REV-2026-16', 'firstClass', 'report-seed+2026-16@snoopy.local', '2026-04-27T14:25:00', 'VISA', '1416', 'Revenue Seed Apr 2'),
    ('REV-2026-17', 'economy',    'report-seed+2026-17@snoopy.local', '2026-05-23T09:40:00', 'MC',   '1517', 'Revenue Seed May 2'),
    ('REV-2026-18', 'firstClass', 'report-seed+2026-18@snoopy.local', '2026-06-29T16:05:00', 'AMEX', '1618', 'Revenue Seed Jun 2'),
    ('REV-2026-19', 'economy',    'report-seed+2026-19@snoopy.local', '2026-07-21T11:20:00', 'VISA', '1719', 'Revenue Seed Jul 2'),
    ('REV-2026-20', 'firstClass', 'report-seed+2026-20@snoopy.local', '2026-08-28T19:55:00', 'MC',   '1820', 'Revenue Seed Aug 2');

INSERT INTO @SeedPassengers (
    OrderKey,
    SequenceNumber,
    Gender,
    FirstName,
    LastName,
    BirthDay,
    BirthMonth,
    BirthYear,
    Nationality,
    CarryOnLuggage,
    CheckedLuggage)
VALUES
    ('REV-2026-01', 1, 'female', 'Sofia',  'Campos',   '05', 'May',      '1990', 'Costa Rica', 1, 1),
    ('REV-2026-01', 2, 'male',   'Diego',  'Rojas',    '14', 'November', '1988', 'Costa Rica', 0, 2),
    ('REV-2026-02', 1, 'female', 'Elena',  'Mora',     '22', 'August',   '1985', 'Spain',      1, 1),
    ('REV-2026-03', 1, 'male',   'Pablo',  'Vega',     '08', 'January',  '1995', 'Costa Rica', 1, 0),
    ('REV-2026-03', 2, 'female', 'Lucia',  'Jimenez',  '17', 'April',    '1993', 'Costa Rica', 1, 1),
    ('REV-2026-03', 3, 'male',   'Mario',  'Solano',   '29', 'June',     '1992', 'Costa Rica', 0, 1),
    ('REV-2026-04', 1, 'female', 'Camila', 'Rivas',    '04', 'February', '1989', 'Panama',     1, 1),
    ('REV-2026-04', 2, 'male',   'Jorge',  'Castro',   '15', 'October',  '1990', 'Panama',     0, 2),
    ('REV-2026-05', 1, 'female', 'Paula',  'Sanchez',  '13', 'March',    '1987', 'Germany',    1, 2),
    ('REV-2026-05', 2, 'male',   'Tomas',  'Sanchez',  '01', 'July',     '1984', 'Germany',    1, 0),
    ('REV-2026-06', 1, 'female', 'Mariana','Lopez',    '23', 'May',      '1996', 'Costa Rica', 1, 1),
    ('REV-2026-06', 2, 'male',   'Andres', 'Vargas',   '12', 'September','1991', 'Costa Rica', 0, 1),
    ('REV-2026-07', 1, 'female', 'Andrea', 'Lopez',    '09', 'December', '1998', 'Costa Rica', 1, 3),
    ('REV-2026-08', 1, 'male',   'Carlos', 'Mena',     '02', 'February', '1986', 'Costa Rica', 1, 1),
    ('REV-2026-08', 2, 'female', 'Laura',  'Solis',    '18', 'May',      '1992', 'Costa Rica', 0, 1),
    ('REV-2026-09', 1, 'female', 'Nadia',  'Ortega',   '11', 'March',    '1994', 'Mexico',     1, 0),
    ('REV-2026-10', 1, 'male',   'Diego',  'Fernandez','27', 'July',     '1988', 'Spain',      1, 2),
    ('REV-2026-10', 2, 'female', 'Ana',    'Mora',     '06', 'November', '1991', 'Spain',      0, 1),
    ('REV-2026-11', 1, 'male',   'Jose',   'Ramirez',  '19', 'January',  '1983', 'Costa Rica', 1, 1),
    ('REV-2026-11', 2, 'female', 'Marta',  'Navarro',  '25', 'August',   '1989', 'Costa Rica', 1, 0),
    ('REV-2026-12', 1, 'female', 'Sonia',  'Paredes',  '14', 'June',     '1990', 'Chile',      1, 2),
    ('REV-2026-12', 2, 'male',   'Esteban','Pizarro',  '30', 'October',  '1987', 'Chile',      0, 1),
    ('REV-2026-13', 1, 'male',   'Kevin',  'Salas',    '08', 'April',    '1995', 'Costa Rica', 1, 0),
    ('REV-2026-14', 1, 'female', 'Daniela','Alfaro',   '21', 'September','1993', 'Costa Rica', 1, 1),
    ('REV-2026-14', 2, 'male',   'Marco',  'Chaves',   '03', 'December', '1985', 'Costa Rica', 0, 2),
    ('REV-2026-15', 1, 'female', 'Elisa',  'Vega',     '17', 'May',      '1996', 'Argentina',  1, 1),
    ('REV-2026-16', 1, 'male',   'Fabian', 'Castillo', '05', 'July',     '1982', 'Argentina',  0, 1),
    ('REV-2026-17', 1, 'female', 'Pilar',  'Guerrero', '12', 'February', '1997', 'Costa Rica', 1, 2),
    ('REV-2026-18', 1, 'male',   'Ricardo','Brenes',   '28', 'March',    '1990', 'Costa Rica', 1, 1),
    ('REV-2026-19', 1, 'female', 'Noelia', 'Perez',    '09', 'August',   '1986', 'Costa Rica', 0, 1),
    ('REV-2026-20', 1, 'male',   'Hector', 'Campos',   '16', 'November', '1992', 'Costa Rica', 1, 3);

INSERT INTO @SeedLegs (OrderKey, SequenceNumber, DepartureCode, ArrivalCode, DepartureAt, ArrivalAt)
VALUES
    ('REV-2026-01', 1, 'SJO', 'MAD', '2026-01-17T07:00:00', '2026-01-17T16:00:00'),
    ('REV-2026-02', 1, 'SJO', 'ATL', '2026-02-05T08:00:00', '2026-02-05T13:00:00'),
    ('REV-2026-02', 2, 'ATL', 'MAD', '2026-02-05T17:00:00', '2026-02-05T23:59:00'),
    ('REV-2026-03', 1, 'SJO', 'CDG', '2026-03-21T06:30:00', '2026-03-21T15:30:00'),
    ('REV-2026-04', 1, 'SJO', 'AMS', '2026-04-09T07:15:00', '2026-04-09T16:15:00'),
    ('REV-2026-05', 1, 'SJO', 'FRA', '2026-05-11T06:45:00', '2026-05-11T15:45:00'),
    ('REV-2026-06', 1, 'SJO', 'MAD', '2026-06-18T07:00:00', '2026-06-18T16:00:00'),
    ('REV-2026-07', 1, 'SJO', 'ATL', '2026-07-03T08:00:00', '2026-07-03T13:00:00'),
    ('REV-2026-08', 1, 'SJO', 'MAD', '2026-08-14T07:00:00', '2026-08-14T16:00:00'),
    ('REV-2026-09', 1, 'SJO', 'CDG', '2026-09-02T06:30:00', '2026-09-02T15:30:00'),
    ('REV-2026-10', 1, 'SJO', 'FRA', '2026-10-19T06:45:00', '2026-10-19T15:45:00'),
    ('REV-2026-11', 1, 'SJO', 'AMS', '2026-11-08T07:15:00', '2026-11-08T16:15:00'),
    ('REV-2026-12', 1, 'SJO', 'MAD', '2026-12-22T07:00:00', '2026-12-22T16:00:00'),
    ('REV-2026-13', 1, 'SJO', 'ATL', '2026-01-28T08:00:00', '2026-01-28T13:00:00'),
    ('REV-2026-14', 1, 'SJO', 'CDG', '2026-02-18T06:30:00', '2026-02-18T15:30:00'),
    ('REV-2026-15', 1, 'SJO', 'FRA', '2026-03-30T06:45:00', '2026-03-30T15:45:00'),
    ('REV-2026-16', 1, 'SJO', 'AMS', '2026-04-27T07:15:00', '2026-04-27T16:15:00'),
    ('REV-2026-17', 1, 'SJO', 'MAD', '2026-05-23T07:00:00', '2026-05-23T16:00:00'),
    ('REV-2026-18', 1, 'SJO', 'ATL', '2026-06-29T08:00:00', '2026-06-29T13:00:00'),
    ('REV-2026-19', 1, 'SJO', 'CDG', '2026-07-21T06:30:00', '2026-07-21T15:30:00'),
    ('REV-2026-20', 1, 'SJO', 'FRA', '2026-08-28T06:45:00', '2026-08-28T15:45:00');

IF EXISTS (
    SELECT 1
    FROM @SeedLegs seed_legs
    LEFT JOIN dbo.airport departure_airport ON departure_airport.code = seed_legs.DepartureCode
    LEFT JOIN dbo.airport arrival_airport ON arrival_airport.code = seed_legs.ArrivalCode
    LEFT JOIN dbo.[route] route
        ON route.departure_airport_id = departure_airport.id
        AND route.arrival_airport_id = arrival_airport.id
        AND route.departure_time = CAST(seed_legs.DepartureAt AS TIME(0))
    WHERE route.id IS NULL
)
BEGIN
    THROW 50000, 'Monthly revenue seed requires the route seed from V32 to be present.', 1;
END;

DECLARE @ExistingPurchaseOrders TABLE (PurchaseOrderId INT NOT NULL PRIMARY KEY);

INSERT INTO @ExistingPurchaseOrders (PurchaseOrderId)
SELECT booking.purchase_order_id
FROM dbo.booking AS booking
INNER JOIN @SeedOrders AS seed_orders
    ON seed_orders.Email = booking.email;

DELETE itinerary
FROM dbo.itinerary AS itinerary
INNER JOIN dbo.booking AS booking
    ON booking.guid = itinerary.booking_guid
INNER JOIN @SeedOrders AS seed_orders
    ON seed_orders.Email = booking.email;

DELETE booking
FROM dbo.booking AS booking
INNER JOIN @SeedOrders AS seed_orders
    ON seed_orders.Email = booking.email;

DELETE passenger
FROM dbo.Passenger AS passenger
INNER JOIN @ExistingPurchaseOrders AS existing_orders
    ON existing_orders.PurchaseOrderId = passenger.PurchaseOrderId;

DELETE purchase_order_route
FROM dbo.PurchaseOrderRoute AS purchase_order_route
INNER JOIN @ExistingPurchaseOrders AS existing_orders
    ON existing_orders.PurchaseOrderId = purchase_order_route.PurchaseOrderId;

DELETE purchase_order
FROM dbo.PurchaseOrder AS purchase_order
INNER JOIN @ExistingPurchaseOrders AS existing_orders
    ON existing_orders.PurchaseOrderId = purchase_order.Id;

DELETE flight
FROM dbo.flight AS flight
INNER JOIN dbo.[route] AS route
    ON route.id = flight.route_id
INNER JOIN dbo.airport AS departure_airport
    ON departure_airport.id = route.departure_airport_id
INNER JOIN dbo.airport AS arrival_airport
    ON arrival_airport.id = route.arrival_airport_id
INNER JOIN @SeedLegs AS seed_legs
    ON seed_legs.DepartureCode = departure_airport.code
    AND seed_legs.ArrivalCode = arrival_airport.code
    AND seed_legs.DepartureAt = flight.departure_at;

DECLARE @OrderKey VARCHAR(40);
DECLARE @SeatClass VARCHAR(50);
DECLARE @Email VARCHAR(254);
DECLARE @ConfirmedAt DATETIME2(0);
DECLARE @CardBrand VARCHAR(30);
DECLARE @CardLastFour CHAR(4);
DECLARE @CardHolderName VARCHAR(120);
DECLARE @PurchaseOrderId INT;
DECLARE @BookingGuid UNIQUEIDENTIFIER;
DECLARE @PassengerCount INT;
DECLARE @CarryOnCount INT;
DECLARE @TicketUnitTotal DECIMAL(18, 2);
DECLARE @CarryOnUnitTotal DECIMAL(18, 2);
DECLARE @CheckedRevenue DECIMAL(18, 2);
DECLARE @TotalAmount DECIMAL(18, 2);

DECLARE SeedOrderCursor CURSOR LOCAL FAST_FORWARD FOR
SELECT OrderKey, SeatClass, Email, ConfirmedAt, CardBrand, CardLastFour, CardHolderName
FROM @SeedOrders
ORDER BY OrderKey;

OPEN SeedOrderCursor;

FETCH NEXT FROM SeedOrderCursor
INTO @OrderKey, @SeatClass, @Email, @ConfirmedAt, @CardBrand, @CardLastFour, @CardHolderName;

WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.PurchaseOrder (SeatClass)
    VALUES (@SeatClass);

    SET @PurchaseOrderId = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.PurchaseOrderRoute (PurchaseOrderId, SequenceNumber, RouteId, IntendedDate)
    SELECT
        @PurchaseOrderId,
        seed_legs.SequenceNumber,
        route.id,
        CAST(seed_legs.DepartureAt AS DATE)
    FROM @SeedLegs AS seed_legs
    INNER JOIN dbo.airport AS departure_airport
        ON departure_airport.code = seed_legs.DepartureCode
    INNER JOIN dbo.airport AS arrival_airport
        ON arrival_airport.code = seed_legs.ArrivalCode
    INNER JOIN dbo.[route] AS route
        ON route.departure_airport_id = departure_airport.id
        AND route.arrival_airport_id = arrival_airport.id
        AND route.departure_time = CAST(seed_legs.DepartureAt AS TIME(0))
    WHERE seed_legs.OrderKey = @OrderKey
    ORDER BY seed_legs.SequenceNumber;

    INSERT INTO dbo.Passenger (
        PurchaseOrderId,
        Gender,
        FirstName,
        LastName,
        BirthDay,
        BirthMonth,
        BirthYear,
        Nationality,
        CarryOnLuggage,
        CheckedLuggage)
    SELECT
        @PurchaseOrderId,
        seed_passengers.Gender,
        seed_passengers.FirstName,
        seed_passengers.LastName,
        seed_passengers.BirthDay,
        seed_passengers.BirthMonth,
        seed_passengers.BirthYear,
        seed_passengers.Nationality,
        seed_passengers.CarryOnLuggage,
        seed_passengers.CheckedLuggage
    FROM @SeedPassengers AS seed_passengers
    WHERE seed_passengers.OrderKey = @OrderKey
    ORDER BY seed_passengers.SequenceNumber;

    INSERT INTO dbo.flight (route_id, departure_at, arrival_at, status, created_at)
    SELECT
        route.id,
        seed_legs.DepartureAt,
        seed_legs.ArrivalAt,
        'completed',
        @ConfirmedAt
    FROM @SeedLegs AS seed_legs
    INNER JOIN dbo.airport AS departure_airport
        ON departure_airport.code = seed_legs.DepartureCode
    INNER JOIN dbo.airport AS arrival_airport
        ON arrival_airport.code = seed_legs.ArrivalCode
    INNER JOIN dbo.[route] AS route
        ON route.departure_airport_id = departure_airport.id
        AND route.arrival_airport_id = arrival_airport.id
        AND route.departure_time = CAST(seed_legs.DepartureAt AS TIME(0))
    WHERE seed_legs.OrderKey = @OrderKey;

    SELECT
        @PassengerCount = COUNT(*),
        @CarryOnCount = COALESCE(SUM(CarryOnLuggage), 0)
    FROM @SeedPassengers
    WHERE OrderKey = @OrderKey;

    SELECT
        @TicketUnitTotal = COALESCE(SUM(
            CASE @SeatClass
                WHEN 'economy' THEN route.price_economy_class
                WHEN 'firstClass' THEN route.price_first_class
                ELSE 0
            END
        ), 0),
        @CarryOnUnitTotal = COALESCE(SUM(route.price_carry_on_baggage), 0)
    FROM @SeedLegs AS seed_legs
    INNER JOIN dbo.airport AS departure_airport
        ON departure_airport.code = seed_legs.DepartureCode
    INNER JOIN dbo.airport AS arrival_airport
        ON arrival_airport.code = seed_legs.ArrivalCode
    INNER JOIN dbo.[route] AS route
        ON route.departure_airport_id = departure_airport.id
        AND route.arrival_airport_id = arrival_airport.id
        AND route.departure_time = CAST(seed_legs.DepartureAt AS TIME(0))
    WHERE seed_legs.OrderKey = @OrderKey;

    SELECT
        @CheckedRevenue = COALESCE(SUM(
            route.price_checked_baggage * (
                CAST(seed_passengers.CheckedLuggage AS DECIMAL(18, 4))
                + ISNULL(route.checked_baggage_price_multiplier, 0) * (
                    CAST(seed_passengers.CheckedLuggage AS DECIMAL(18, 4))
                    * (CAST(seed_passengers.CheckedLuggage AS DECIMAL(18, 4)) - 1)
                    / 2.0
                )
            )
        ), 0)
    FROM @SeedPassengers AS seed_passengers
    CROSS JOIN @SeedLegs AS seed_legs
    INNER JOIN dbo.airport AS departure_airport
        ON departure_airport.code = seed_legs.DepartureCode
    INNER JOIN dbo.airport AS arrival_airport
        ON arrival_airport.code = seed_legs.ArrivalCode
    INNER JOIN dbo.[route] AS route
        ON route.departure_airport_id = departure_airport.id
        AND route.arrival_airport_id = arrival_airport.id
        AND route.departure_time = CAST(seed_legs.DepartureAt AS TIME(0))
    WHERE seed_passengers.OrderKey = @OrderKey
      AND seed_legs.OrderKey = @OrderKey
      AND seed_passengers.CheckedLuggage > 0;

    SET @TotalAmount = CAST((@TicketUnitTotal * @PassengerCount) + (@CarryOnUnitTotal * @CarryOnCount) + COALESCE(@CheckedRevenue, 0) AS DECIMAL(18, 2));
    SET @BookingGuid = NEWID();

    INSERT INTO dbo.booking (
        guid,
        purchase_order_id,
        confirmation_code,
        email,
        status,
        total_amount,
        card_brand,
        card_last_four,
        card_holder_name,
        created_at,
        confirmed_at)
    VALUES (
        @BookingGuid,
        @PurchaseOrderId,
        REPLACE(@OrderKey, '-', ''),
        @Email,
        'confirmed',
        @TotalAmount,
        @CardBrand,
        @CardLastFour,
        @CardHolderName,
        @ConfirmedAt,
        @ConfirmedAt);

    INSERT INTO dbo.itinerary (booking_guid, sequence_number, flight_guid)
    SELECT
        @BookingGuid,
        seed_legs.SequenceNumber,
        flight.guid
    FROM @SeedLegs AS seed_legs
    INNER JOIN dbo.airport AS departure_airport
        ON departure_airport.code = seed_legs.DepartureCode
    INNER JOIN dbo.airport AS arrival_airport
        ON arrival_airport.code = seed_legs.ArrivalCode
    INNER JOIN dbo.[route] AS route
        ON route.departure_airport_id = departure_airport.id
        AND route.arrival_airport_id = arrival_airport.id
        AND route.departure_time = CAST(seed_legs.DepartureAt AS TIME(0))
    INNER JOIN dbo.flight AS flight
        ON flight.route_id = route.id
        AND flight.departure_at = seed_legs.DepartureAt
    WHERE seed_legs.OrderKey = @OrderKey
    ORDER BY seed_legs.SequenceNumber;

    FETCH NEXT FROM SeedOrderCursor
    INTO @OrderKey, @SeatClass, @Email, @ConfirmedAt, @CardBrand, @CardLastFour, @CardHolderName;
END;

CLOSE SeedOrderCursor;
DEALLOCATE SeedOrderCursor;

COMMIT TRANSACTION;