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
        SUM(CASE
            WHEN po.SeatClass = 'firstClass' THEN COALESCE(r.price_first_class, fe.first_class_price, 0)
            ELSE COALESCE(r.price_economy_class, fe.tourist_price, 0)
        END) AS VentaPasajeros,
        SUM((
            ISNULL(TRY_CAST(p.CarryOnLuggage AS DECIMAL(10, 2)), 0)
                * COALESCE(r.price_carry_on_baggage, fe.carry_on_price, 0)
            + (
                ISNULL(TRY_CAST(p.CheckedLuggage AS DECIMAL(10, 2)), 0)
                + ISNULL(r.checked_baggage_price_multiplier, 0)
                    * (ISNULL(TRY_CAST(p.CheckedLuggage AS DECIMAL(10, 2)), 0)
                       * (ISNULL(TRY_CAST(p.CheckedLuggage AS DECIMAL(10, 2)), 0) - 1) / 2.0)
              ) * COALESCE(r.price_checked_baggage, fe.checked_price, 0)
        ) / CAST(lc.total_legs AS DECIMAL(10, 2))) AS VentaEquipajes,
        SUM(CASE
            WHEN po.SeatClass = 'firstClass' THEN COALESCE(r.price_first_class, fe.first_class_price, 0)
            ELSE COALESCE(r.price_economy_class, fe.tourist_price, 0)
        END) +
        SUM((
            ISNULL(TRY_CAST(p.CarryOnLuggage AS DECIMAL(10, 2)), 0)
                * COALESCE(r.price_carry_on_baggage, fe.carry_on_price, 0)
            + (
                ISNULL(TRY_CAST(p.CheckedLuggage AS DECIMAL(10, 2)), 0)
                + ISNULL(r.checked_baggage_price_multiplier, 0)
                    * (ISNULL(TRY_CAST(p.CheckedLuggage AS DECIMAL(10, 2)), 0)
                       * (ISNULL(TRY_CAST(p.CheckedLuggage AS DECIMAL(10, 2)), 0) - 1) / 2.0)
              ) * COALESCE(r.price_checked_baggage, fe.checked_price, 0)
        ) / CAST(lc.total_legs AS DECIMAL(10, 2))) AS TotalVenta
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
