CREATE OR ALTER FUNCTION dbo.GetAirlineDetailedReport(
    @origin CHAR(3),
    @destination CHAR(3),
    @seat_class VARCHAR(20),
    @date_from DATE,
    @date_to DATE
)
RETURNS TABLE
AS
RETURN (
    SELECT
        CAST(f.departure_at AS DATE) AS Fecha,
        da.code AS Origen,
        aa.code AS Destino,
        r.flight_code AS FlightCode,
        COUNT(CASE WHEN po.SeatClass = 'firstClass' THEN p.Id END) AS PasajerosPrimeraClase,
        COUNT(CASE WHEN po.SeatClass = 'economy' THEN p.Id END) AS PasajerosEconomia,
        b.airline_name AS Aerolinea,
        SUM(inf.unit_price) AS VentaPasajeros,
        SUM((il.carry_on_total + il.checked_total) / CAST(lc.total_legs AS DECIMAL(10, 2))) AS VentaEquipajes,
        SUM(inf.unit_price) + SUM((il.carry_on_total + il.checked_total) / CAST(lc.total_legs AS DECIMAL(10, 2))) AS TotalVenta
    FROM dbo.flight f
    JOIN dbo.[route] r ON r.id = f.route_id
    JOIN dbo.airport da ON da.id = r.departure_airport_id
    JOIN dbo.airport aa ON aa.id = r.arrival_airport_id
    JOIN dbo.itinerary i ON i.flight_guid = f.guid
    JOIN dbo.booking b ON b.guid = i.booking_guid
    JOIN dbo.PurchaseOrder po ON po.Id = b.purchase_order_id
    JOIN dbo.Passenger p ON p.PurchaseOrderId = po.Id
    JOIN dbo.invoice inv ON inv.booking_guid = b.guid
    JOIN dbo.invoice_flight inf ON inf.invoice_id = inv.id AND inf.flight_guid = f.guid AND inf.passenger_id = p.Id
    JOIN dbo.invoice_luggage il ON il.invoice_id = inv.id AND il.passenger_id = p.Id
    JOIN (
        SELECT booking_guid, COUNT(*) AS total_legs
        FROM dbo.itinerary
        GROUP BY booking_guid
    ) lc ON lc.booking_guid = b.guid
    WHERE
        b.status NOT IN ('cancelled', 'refunded')
        AND (@origin IS NULL OR da.code = @origin)
        AND (@destination IS NULL OR aa.code = @destination)
        AND (@date_from IS NULL OR CAST(f.departure_at AS DATE) >= @date_from)
        AND (@date_to IS NULL OR CAST(f.departure_at AS DATE) <= @date_to)
    GROUP BY
        CAST(f.departure_at AS DATE),
        da.code,
        aa.code,
        r.flight_code,
        b.airline_name,
        f.guid
    HAVING
        @seat_class IS NULL
        OR SUM(CASE WHEN po.SeatClass = @seat_class THEN 1 ELSE 0 END) > 0
);
GO
