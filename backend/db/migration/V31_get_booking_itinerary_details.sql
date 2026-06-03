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
        JOIN dbo.PurchaseOrder po    ON po.Id          = b.purchase_order_id
        JOIN dbo.itinerary i         ON i.booking_guid = b.guid
        JOIN dbo.flight f            ON f.guid         = i.flight_guid
        JOIN dbo.[route] r           ON r.id           = f.route_id
        JOIN dbo.airport da          ON da.id          = r.departure_airport_id
        JOIN dbo.city dc             ON dc.id          = da.city_id
        JOIN dbo.airport aa          ON aa.id          = r.arrival_airport_id
        JOIN dbo.city ac             ON ac.id          = aa.city_id
        JOIN dbo.airplane ap         ON ap.id          = r.airplane_id
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

        -- Origen: primer leg
        fl.DepartureAirportName,
        fl.DepartureAirportCode,
        fl.DepartureCityName,
        fl.DepartureAt,

        -- Destino: último leg
        ll.ArrivalAirportName,
        ll.ArrivalAirportCode,
        ll.ArrivalCityName,
        ll.ArrivalAt,

        -- Duracion total puerta a puerta
        DATEDIFF(MINUTE, fl.DepartureAt, ll.ArrivalAt) AS TotalItineraryMinutes,

        -- Suma solo de los tiempos en vuelo (sin contar escalas)
        (SELECT SUM(LegDurationMinutes) FROM legs)      AS TotalFlightMinutes,

        -- Numero de escalas
        (SELECT COUNT(*) - 1 FROM legs)                 AS LayoverCount

    FROM first_leg fl
    CROSS JOIN last_leg ll
);