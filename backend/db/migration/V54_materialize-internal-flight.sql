CREATE OR ALTER PROCEDURE dbo.materializeFlight
    @RouteId     INT,
    @DepartureAt DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @flight_guid UNIQUEIDENTIFIER;
    DECLARE @route_departure_time TIME(0);
    DECLARE @route_arrival_time TIME(0);
    DECLARE @arrival_at DATETIME2(0);

    BEGIN TRY
        IF @RouteId IS NULL OR @RouteId <= 0
        BEGIN
            THROW 50000, 'RouteId must be greater than zero.', 1;
        END;

        IF @DepartureAt IS NULL
        BEGIN
            THROW 50000, 'DepartureAt is required.', 1;
        END;

        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

        BEGIN TRANSACTION;

        SELECT @flight_guid = flight.guid
        FROM dbo.flight flight
        INNER JOIN dbo.flight_internal internal_flight
            ON internal_flight.flight_guid = flight.guid
        WHERE internal_flight.route_id = @RouteId
          AND flight.departure_at = @DepartureAt;

        IF @flight_guid IS NULL
        BEGIN
            SELECT
                @route_departure_time = route.departure_time,
                @route_arrival_time = route.arrival_time
            FROM dbo.[route] route
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

            IF @flight_guid IS NULL
            BEGIN
                THROW 50000, 'Flight insert did not return a guid.', 1;
            END;

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

        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

        SELECT @flight_guid AS FlightGuid;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

        THROW;
    END CATCH;
END;
GO
