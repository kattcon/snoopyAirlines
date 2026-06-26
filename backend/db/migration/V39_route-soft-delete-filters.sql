CREATE OR ALTER FUNCTION dbo.route_flights_at (
    @departure_date DATE
)
RETURNS TABLE
AS
RETURN
    SELECT
        route.id AS route_id,
        DATEADD(
            SECOND,
            DATEDIFF(SECOND, CONVERT(time(0), '00:00:00'), route.departure_time),
            CONVERT(datetime2(0), @departure_date)
        ) AS departure_at,
        DATEADD(
            SECOND,
            DATEDIFF(SECOND, CONVERT(time(0), '00:00:00'), route.arrival_time),
            DATEADD(
                DAY,
                CASE WHEN route.arrival_time >= route.departure_time THEN 0 ELSE 1 END,
                CONVERT(datetime2(0), @departure_date)
            )
        ) AS arrival_at
    FROM dbo.[route] AS route
    WHERE route.is_deleted = 0
      AND (route.frequency & CASE ((DATEDIFF(DAY, CONVERT(date, '19000101'), @departure_date) % 7) + 7) % 7
        WHEN 0 THEN 64 -- Monday
        WHEN 1 THEN 32 -- Tuesday
        WHEN 2 THEN 16 -- Wednesday
        WHEN 3 THEN 8  -- Thursday
        WHEN 4 THEN 4  -- Friday
        WHEN 5 THEN 2  -- Saturday
        WHEN 6 THEN 1  -- Sunday
    END) <> 0;
GO

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
        INNER JOIN dbo.[route] ON [route].id = flight.route_id
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

CREATE OR ALTER TRIGGER dbo.TR_No_duplicate_routes
ON dbo.[route]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.[route] r
            ON r.departure_airport_id = i.departure_airport_id
           AND r.arrival_airport_id = i.arrival_airport_id
           AND r.departure_time = i.departure_time
           AND r.arrival_time = i.arrival_time
           AND r.frequency = i.frequency
           AND r.id <> i.id
        WHERE i.is_deleted = 0
          AND r.is_deleted = 0
    )
    BEGIN
        RAISERROR('Ya existe una ruta con los datos proporcionados', 16, 1);
        ROLLBACK TRANSACTION;
    END;
END;
GO

-- Keep the latest booking procedure from V36, but make its route lookup
-- active-only. route_flights_at is also filtered above so a route deleted
-- between the lookup and schedule calculation cannot be booked.
DECLARE @book_definition NVARCHAR(MAX) =
    OBJECT_DEFINITION(OBJECT_ID(N'dbo.book', N'P'));
DECLARE @old_route_join NVARCHAR(200) =
    N'ON r.id = l.route_id;';
DECLARE @active_route_join NVARCHAR(200) =
    N'ON r.id = l.route_id AND r.is_deleted = 0;';

IF @book_definition IS NULL
BEGIN
    THROW 50000, 'dbo.book was not found while applying route soft-delete filters.', 1;
END;

IF CHARINDEX(N'r.is_deleted = 0', @book_definition) = 0
BEGIN
    IF CHARINDEX(@old_route_join, @book_definition) = 0
    BEGIN
        THROW 50000, 'The dbo.book route lookup could not be updated safely.', 1;
    END;

    SET @book_definition = REPLACE(
        @book_definition,
        @old_route_join,
        @active_route_join);

    DECLARE @create_position INT = CHARINDEX(N'CREATE', @book_definition);
    DECLARE @procedure_position INT =
        CHARINDEX(N'PROCEDURE', @book_definition, @create_position);

    IF @create_position = 0 OR @procedure_position = 0
    BEGIN
        THROW 50000, 'The dbo.book definition could not be converted to ALTER PROCEDURE.', 1;
    END;

    SET @book_definition = STUFF(
        @book_definition,
        @create_position,
        @procedure_position - @create_position,
        N'ALTER ');

    EXEC sys.sp_executesql @book_definition;
END;
GO

-- Booking itinerary and customer flight-report functions intentionally keep
-- deleted routes visible because they describe already-purchased history.
