CREATE FUNCTION GetPurchaseOrderDetails(@purchaseOrderId INT)
RETURNS TABLE
AS
RETURN (
    SELECT
        -- Purchase Order
        po.Id               AS PurchaseOrderId,
        po.SeatClass,
        po.Status,

        -- Flight
        f.id                AS FlightId,
        f.departure_time    AS DepartureTime,
        f.arrival_time      AS ArrivalTime,
        f.duration_minutes  AS DurationMinutes,

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

    FROM PurchaseOrder po
    JOIN flight f               ON f.id = po.FlightId
    JOIN airport da             ON da.id = f.departure_airport_id
    JOIN city dc                ON dc.id = da.city_id
    JOIN airport aa             ON aa.id = f.arrival_airport_id
    JOIN city ac                ON ac.id = aa.city_id
    JOIN airplane ap            ON ap.id = f.airplane_id
    JOIN Passenger p            ON p.PurchaseOrderId = po.Id
    WHERE po.Id = @purchaseOrderId
);