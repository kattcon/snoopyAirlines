CREATE OR ALTER PROCEDURE dbo.materializeFlight
    @RouteId     INT,
    @DepartureAt DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    DECLARE @flight_guid UNIQUEIDENTIFIER;
    DECLARE @route_departure_time TIME(0);
    DECLARE @route_arrival_time TIME(0);
    DECLARE @arrival_at DATETIME2(0);

    BEGIN TRANSACTION;

    SELECT @flight_guid = flight.guid
    FROM dbo.flight flight WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.flight_internal internal_flight WITH (UPDLOCK, HOLDLOCK)
        ON internal_flight.flight_guid = flight.guid
    WHERE internal_flight.route_id = @RouteId
      AND flight.departure_at = @DepartureAt;

    IF @flight_guid IS NULL
    BEGIN
        SELECT
            @route_departure_time = route.departure_time,
            @route_arrival_time = route.arrival_time
        FROM dbo.[route] route WITH (UPDLOCK, HOLDLOCK)
        WHERE route.id = @RouteId
          AND route.is_deleted = 0;

        IF @route_departure_time IS NULL
        BEGIN
            THROW 50000, 'Route not found.', 1;
        END;

        IF CAST(@DepartureAt AS TIME(0)) <> @route_departure_time
        BEGIN
            THROW 50000, 'Departure time does not match the route schedule.', 1;
        END;

        SET @arrival_at = DATEADD(
            SECOND,
            DATEDIFF(SECOND, CAST('00:00:00' AS TIME(0)), @route_arrival_time),
            DATEADD(
                DAY,
                CASE WHEN @route_arrival_time < @route_departure_time THEN 1 ELSE 0 END,
                CONVERT(DATETIME2(0), CONVERT(DATE, @DepartureAt))
            )
        );

        DECLARE @inserted_flight TABLE (
            guid UNIQUEIDENTIFIER NOT NULL
        );

        INSERT INTO dbo.flight (
            departure_at,
            arrival_at
        )
        OUTPUT inserted.guid INTO @inserted_flight
        VALUES (
            @DepartureAt,
            @arrival_at
        );

        SELECT TOP (1) @flight_guid = guid
        FROM @inserted_flight;

        INSERT INTO dbo.flight_internal (
            flight_guid,
            route_id
        )
        VALUES (
            @flight_guid,
            @RouteId
        );
    END;

    COMMIT TRANSACTION;

    SELECT @flight_guid AS FlightGuid;
END;
GO
