CREATE OR ALTER FUNCTION [dbo].[GetFlightReportByConfirmation](
    @confirmation_code VARCHAR(12),
    @last_name_search  VARCHAR(120)
)
RETURNS TABLE
AS
RETURN (
    SELECT
        b.confirmation_code                             AS ReservationNumber,
        b.card_holder_name                             AS CardHolderName,
        b.status                                       AS BookingStatus,
        i.sequence_number                              AS SequenceNumber,

        dc.name                                        AS DepartureCity,
        ac.name                                        AS ArrivalCity,
        da.code                                        AS DepartureAirportCode,
        aa.code                                        AS ArrivalAirportCode,

        CAST(f.departure_at AS DATE)                   AS DepartureDate,
        CAST(f.departure_at AS TIME(0))                AS DepartureTime,
        CAST(f.arrival_at AS DATE)                     AS ArrivalDate,
        CAST(f.arrival_at AS TIME(0))                  AS ArrivalTime,

        r.duration_minutes                             AS DurationMinutes,
        ap.model                                       AS AirplaneModel,

        (
            SELECT COUNT(*)
            FROM dbo.Passenger p
            WHERE p.PurchaseOrderId = po.Id
        )                                               AS PassengerCount

    FROM dbo.booking b
    JOIN dbo.PurchaseOrder po   ON po.Id            = b.purchase_order_id
    JOIN dbo.itinerary i        ON i.booking_guid   = b.guid
    JOIN dbo.flight f           ON f.guid           = i.flight_guid

    JOIN dbo.[route] r          ON r.id             = f.route_id
    JOIN dbo.airport da         ON da.id            = r.departure_airport_id
    JOIN dbo.city dc            ON dc.id            = da.city_id
    JOIN dbo.airport aa         ON aa.id            = r.arrival_airport_id
    JOIN dbo.city ac            ON ac.id            = aa.city_id
    JOIN dbo.airplane ap        ON ap.id            = r.airplane_id

    WHERE
        b.confirmation_code = UPPER(LTRIM(RTRIM(@confirmation_code)))
        AND REVERSE(
                LEFT(
                    REVERSE(LTRIM(RTRIM(b.card_holder_name))),
                    CASE
                        WHEN CHARINDEX(' ', REVERSE(LTRIM(RTRIM(b.card_holder_name)))) > 0
                        THEN CHARINDEX(' ', REVERSE(LTRIM(RTRIM(b.card_holder_name)))) - 1
                        ELSE LEN(LTRIM(RTRIM(b.card_holder_name)))
                    END
                )
            ) = LTRIM(RTRIM(@last_name_search))

        AND LTRIM(RTRIM(@last_name_search)) <> ''
);
GO