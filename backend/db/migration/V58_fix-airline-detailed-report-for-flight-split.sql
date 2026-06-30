CREATE OR ALTER FUNCTION dbo.GetAirlineDetailedReport(
    @origin CHAR(3),
    @destination CHAR(3),
    @seat_class VARCHAR(20),
    @date_from DATE,
    @date_to DATE,
    @airline_name VARCHAR(120)
)
RETURNS TABLE
AS
RETURN (
    SELECT
        CAST(f.departure_at AS DATE) AS Fecha,
        COALESCE(da.code, fe.departure_airport_code) AS Origen,
        COALESCE(aa.code, fe.arrival_airport_code) AS Destino,
        r.flight_code AS FlightCode,
        COUNT(CASE WHEN po.SeatClass = 'firstClass' THEN p.Id END) AS PasajerosPrimeraClase,
        COUNT(CASE WHEN po.SeatClass = 'economy' THEN p.Id END) AS PasajerosEconomia,
        COALESCE(pa.name, 'Snoopy Airlines') AS Aerolinea,
        COALESCE(SUM(inf.unit_price), 0) AS VentaPasajeros,
        COALESCE(
            SUM((il.carry_on_total + il.checked_total) / CAST(lc.total_legs AS DECIMAL(10, 2))),
            0
        ) AS VentaEquipajes,
        COALESCE(SUM(inf.unit_price), 0) +
        COALESCE(
            SUM((il.carry_on_total + il.checked_total) / CAST(lc.total_legs AS DECIMAL(10, 2))),
            0
        ) AS TotalVenta
    FROM dbo.flight f
    LEFT JOIN dbo.flight_internal fi ON fi.flight_guid = f.guid
    LEFT JOIN dbo.[route] r ON r.id = fi.route_id
    LEFT JOIN dbo.airport da ON da.id = r.departure_airport_id
    LEFT JOIN dbo.airport aa ON aa.id = r.arrival_airport_id
    LEFT JOIN dbo.flight_external fe ON fe.flight_guid = f.guid
    LEFT JOIN dbo.partner_airline pa ON pa.id = fe.partner_airline_id
    JOIN dbo.itinerary i ON i.flight_guid = f.guid
    JOIN dbo.booking b ON b.guid = i.booking_guid
    JOIN dbo.PurchaseOrder po ON po.Id = b.purchase_order_id
    JOIN dbo.Passenger p ON p.PurchaseOrderId = po.Id
    LEFT JOIN dbo.invoice inv ON inv.booking_guid = b.guid
    LEFT JOIN dbo.invoice_flight inf
        ON inf.invoice_id = inv.id
        AND inf.flight_guid = f.guid
        AND inf.passenger_id = p.Id
    LEFT JOIN dbo.invoice_luggage il
        ON il.invoice_id = inv.id
        AND il.passenger_id = p.Id
    LEFT JOIN (
        SELECT booking_guid, COUNT(*) AS total_legs
        FROM dbo.itinerary
        GROUP BY booking_guid
    ) lc ON lc.booking_guid = b.guid
    WHERE
        b.status NOT IN ('cancelled', 'refunded')
        AND (@origin IS NULL OR COALESCE(da.code, fe.departure_airport_code) = @origin)
        AND (@destination IS NULL OR COALESCE(aa.code, fe.arrival_airport_code) = @destination)
        AND (@date_from IS NULL OR CAST(f.departure_at AS DATE) >= @date_from)
        AND (@date_to IS NULL OR CAST(f.departure_at AS DATE) <= @date_to)
        AND (@airline_name IS NULL OR COALESCE(pa.name, 'Snoopy Airlines') = @airline_name)
    GROUP BY
        CAST(f.departure_at AS DATE),
        COALESCE(da.code, fe.departure_airport_code),
        COALESCE(aa.code, fe.arrival_airport_code),
        r.flight_code,
        COALESCE(pa.name, 'Snoopy Airlines'),
        f.guid
    HAVING
        @seat_class IS NULL
        OR SUM(CASE WHEN po.SeatClass = @seat_class THEN 1 ELSE 0 END) > 0
);
GO
