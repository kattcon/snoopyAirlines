CREATE OR ALTER FUNCTION [dbo].[GetFlightReportByConfirmation](
    @confirmation_code VARCHAR(12),
    @last_name_search  VARCHAR(120)
)
RETURNS TABLE
AS
RETURN (
    SELECT
        b.confirmation_code                AS NumeroReservacion,
        b.card_holder_name                 AS TitularReservacion,
        i.sequence_number                  AS NumeroTramo,

        dc.name                            AS CiudadPartida,
        ac.name                            AS CiudadLlegada,

        da.code                            AS CodigoAeropuertoSalida,
        aa.code                            AS CodigoAeropuertoLlegada,

        CAST(f.departure_at AS DATE)       AS FechaSalida,
        CAST(f.departure_at AS TIME(0))    AS HoraSalida,
        CAST(f.arrival_at AS DATE)         AS FechaLlegada,
        CAST(f.arrival_at AS TIME(0))      AS HoraLlegada,

        r.duration_minutes                 AS DuracionVueloMinutos,

        ap.model                           AS Aeronave,

        (
            SELECT COUNT(*)
            FROM dbo.Passenger p
            WHERE p.PurchaseOrderId = po.Id
        )                                   AS CantidadPasajeros

    FROM dbo.booking b
    JOIN dbo.PurchaseOrder po  ON po.Id          = b.purchase_order_id
    JOIN dbo.itinerary i       ON i.booking_guid = b.guid
    JOIN dbo.flight f          ON f.guid         = i.flight_guid
    JOIN dbo.[route] r         ON r.id           = f.route_id
    JOIN dbo.airport da        ON da.id          = r.departure_airport_id
    JOIN dbo.city dc           ON dc.id          = da.city_id
    JOIN dbo.airport aa        ON aa.id          = r.arrival_airport_id
    JOIN dbo.city ac           ON ac.id          = aa.city_id
    JOIN dbo.airplane ap       ON ap.id          = r.airplane_id
    WHERE
        b.confirmation_code = UPPER(LTRIM(RTRIM(@confirmation_code)))
        AND b.card_holder_name LIKE
            '%' + REPLACE(REPLACE(LTRIM(RTRIM(@last_name_search)), '[', '[[]'), '%', '[%]') + '%'
);
