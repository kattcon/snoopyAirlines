CREATE OR ALTER FUNCTION GetPurchaseOrderDetails(@purchaseOrderId INT)
RETURNS TABLE
AS
RETURN (
    SELECT
        -- Purchase Order
        po.Id               AS PurchaseOrderId,
        po.SeatClass,
        po.IntendedDate,

        -- Route
        r.id                AS RouteId,
        r.departure_time    AS DepartureTime,
        r.arrival_time      AS ArrivalTime,
        r.duration_minutes  AS DurationMinutes,

        -- Flight
        f.guid              AS FlightGuid,
        f.departure_at      AS DepartureAt,
        f.arrival_at        AS ArrivalAt,
        f.status            AS FlightStatus,

        -- Departure Airport
        da.name             AS DepartureAirportName,
        da.code             AS DepartureAirportCode,
        dc.name             AS DepartureCityName,

        -- Arrival Airport
        aa.name             AS ArrivalAirportName,
        aa.code             AS ArrivalAirportCode,
        ac.name             AS ArrivalCityName,

        -- Airplane
        ap.model            AS AirplaneModel,

        -- Passengers
        p.Id                AS PassengerId,
        p.FirstName,
        p.LastName,
        p.Gender,
        p.Nationality,
        p.BirthDay,
        p.BirthMonth,
        p.BirthYear

    FROM dbo.PurchaseOrder po
    JOIN dbo.[route] r          ON r.id = po.RouteId
    LEFT JOIN dbo.flight f      ON f.route_id = r.id
                                AND f.departure_at = DATEADD(
                                        SECOND,
                                        DATEDIFF(SECOND, CONVERT(TIME(0), '00:00:00'), r.departure_time),
                                        CONVERT(DATETIME2(0), po.IntendedDate)
                                    )
    JOIN dbo.airport da         ON da.id = r.departure_airport_id
    JOIN dbo.city dc            ON dc.id = da.city_id
    JOIN dbo.airport aa         ON aa.id = r.arrival_airport_id
    JOIN dbo.city ac            ON ac.id = aa.city_id
    JOIN dbo.airplane ap        ON ap.id = r.airplane_id
    JOIN dbo.Passenger p        ON p.PurchaseOrderId = po.Id
    WHERE po.Id = @purchaseOrderId
);