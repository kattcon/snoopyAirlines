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

        COALESCE(dc.name, fe.departure_city)           AS DepartureCity,

        COALESCE(ac.name, fe.arrival_city)             AS ArrivalCity,

        COALESCE(da.code, fe.departure_airport_code)   AS DepartureAirportCode,

        COALESCE(aa.code, fe.arrival_airport_code)     AS ArrivalAirportCode,

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

    LEFT JOIN dbo.flight_internal fi    ON fi.flight_guid           = f.guid
    LEFT JOIN dbo.[route] r             ON r.id                     = fi.route_id
    LEFT JOIN dbo.airport da            ON da.id                    = r.departure_airport_id
    LEFT JOIN dbo.city dc               ON dc.id                    = da.city_id
    LEFT JOIN dbo.airport aa            ON aa.id                    = r.arrival_airport_id
    LEFT JOIN dbo.city ac               ON ac.id                    = aa.city_id
    LEFT JOIN dbo.airplane ap           ON ap.id                    = r.airplane_id

    LEFT JOIN dbo.flight_external fe    ON fe.flight_guid           = f.guid

    WHERE
        b.confirmation_code = UPPER(LTRIM(RTRIM(@confirmation_code)))

        AND (
            CASE
                WHEN CHARINDEX(' ', LTRIM(RTRIM(@last_name_search))) = 0
                THEN
                    CASE
                        WHEN REVERSE(
                                LEFT(
                                    REVERSE(LTRIM(RTRIM(b.card_holder_name))),
                                    CASE
                                        WHEN CHARINDEX(' ', REVERSE(LTRIM(RTRIM(b.card_holder_name)))) > 0
                                        THEN CHARINDEX(' ', REVERSE(LTRIM(RTRIM(b.card_holder_name)))) - 1
                                        ELSE LEN(LTRIM(RTRIM(b.card_holder_name)))
                                    END
                                )
                             ) = LTRIM(RTRIM(@last_name_search))
                        THEN 1 ELSE 0
                    END
                ELSE
                    CASE
                        WHEN REVERSE(
                                LEFT(
                                    REVERSE(LTRIM(RTRIM(b.card_holder_name))),
                                    CASE
                                        WHEN CHARINDEX(' ',
                                                REVERSE(LTRIM(RTRIM(b.card_holder_name))),
                                                CHARINDEX(' ', REVERSE(LTRIM(RTRIM(b.card_holder_name)))) + 1
                                             ) > 0
                                        THEN CHARINDEX(' ',
                                                REVERSE(LTRIM(RTRIM(b.card_holder_name))),
                                                CHARINDEX(' ', REVERSE(LTRIM(RTRIM(b.card_holder_name)))) + 1
                                             ) - 1
                                        ELSE LEN(LTRIM(RTRIM(b.card_holder_name)))
                                    END
                                )
                             ) = LTRIM(RTRIM(@last_name_search))
                        THEN 1 ELSE 0
                    END
            END
        ) = 1

        AND LTRIM(RTRIM(@last_name_search)) <> ''
);
GO