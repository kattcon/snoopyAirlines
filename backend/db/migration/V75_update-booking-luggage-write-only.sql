CREATE OR ALTER PROCEDURE dbo.UpdateBookingLuggage
    @ConfirmationCode VARCHAR(12),
    @Passengers       dbo.PassengerLuggageType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRANSACTION;

    BEGIN TRY

        IF NOT EXISTS (
            SELECT 1 FROM dbo.booking
            WHERE confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationCode)))
        )
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50001, 'No reservation was found with that confirmation code.', 1;
        END

        IF EXISTS (
            SELECT 1
            FROM @Passengers
            WHERE NewCheckedLuggage < 0
        )
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50003, 'Checked luggage cannot be negative.', 1;
        END

        IF EXISTS (
            SELECT 1 FROM @Passengers p
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.booking b
                JOIN dbo.Passenger pas ON pas.PurchaseOrderId = b.purchase_order_id
                WHERE b.confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationCode)))
                AND pas.Id = p.Id
            )
        )
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50002, 'One or more passengers do not belong to this reservation.', 1;
        END

        DECLARE @capacity_violations TABLE (
            FlightGuid UNIQUEIDENTIFIER NOT NULL
        );

        ;WITH target_booking AS (
            SELECT guid, purchase_order_id
            FROM dbo.booking
            WHERE confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationCode)))
        ),
        target_flights AS (
            SELECT
                itinerary.flight_guid AS FlightGuid,
                route.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
                route.weight_limit_checked_baggage AS WeightLimitCheckedBaggage,
                CAST(airplane.max_weight AS DECIMAL(18, 2)) AS AirplaneMaxWeight
            FROM target_booking
            JOIN dbo.itinerary itinerary
                ON itinerary.booking_guid = target_booking.guid
            JOIN dbo.flight_internal internal_flight
                ON internal_flight.flight_guid = itinerary.flight_guid
            JOIN dbo.[route] route
                ON route.id = internal_flight.route_id
                AND route.is_deleted = 0
            JOIN dbo.airplane airplane
                ON airplane.id = route.airplane_id
                AND airplane.is_deleted = 0
        ),
        other_booked_luggage AS (
            SELECT
                target_flights.FlightGuid,
                SUM(
                    CAST(passenger.CarryOnLuggage AS DECIMAL(18, 2)) * target_flights.WeightLimitCarryOnBaggage
                    + CAST(passenger.CheckedLuggage AS DECIMAL(18, 2)) * target_flights.WeightLimitCheckedBaggage
                ) AS BookedLuggageWeight
            FROM target_flights
            JOIN dbo.itinerary itinerary
                ON itinerary.flight_guid = target_flights.FlightGuid
            JOIN dbo.booking booking
                ON booking.guid = itinerary.booking_guid
                AND booking.status = 'confirmed'
            JOIN target_booking
                ON target_booking.guid <> booking.guid
            JOIN dbo.Passenger passenger
                ON passenger.PurchaseOrderId = booking.purchase_order_id
            GROUP BY target_flights.FlightGuid
        ),
        updated_booking_luggage AS (
            SELECT
                target_flights.FlightGuid,
                SUM(
                    CAST(passenger.CarryOnLuggage AS DECIMAL(18, 2)) * target_flights.WeightLimitCarryOnBaggage
                    + CAST(COALESCE(passenger_update.NewCheckedLuggage, passenger.CheckedLuggage) AS DECIMAL(18, 2)) * target_flights.WeightLimitCheckedBaggage
                ) AS UpdatedBookingLuggageWeight
            FROM target_flights
            CROSS JOIN target_booking
            JOIN dbo.Passenger passenger
                ON passenger.PurchaseOrderId = target_booking.purchase_order_id
            LEFT JOIN @Passengers passenger_update
                ON passenger_update.Id = passenger.Id
            GROUP BY target_flights.FlightGuid
        )
        INSERT INTO @capacity_violations (FlightGuid)
        SELECT target_flights.FlightGuid
        FROM target_flights
        LEFT JOIN other_booked_luggage
            ON other_booked_luggage.FlightGuid = target_flights.FlightGuid
        LEFT JOIN updated_booking_luggage
            ON updated_booking_luggage.FlightGuid = target_flights.FlightGuid
        WHERE
            COALESCE(other_booked_luggage.BookedLuggageWeight, 0)
            + COALESCE(updated_booking_luggage.UpdatedBookingLuggageWeight, 0)
            > target_flights.AirplaneMaxWeight;

        IF EXISTS (SELECT 1 FROM @capacity_violations)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50003, 'Luggage weight exceeds the airplane maximum weight.', 1;
        END

        UPDATE pas
        SET pas.CheckedLuggage = p.NewCheckedLuggage
        FROM dbo.Passenger pas
        JOIN @Passengers p ON p.Id = pas.Id;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
