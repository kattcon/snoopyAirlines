CREATE OR ALTER FUNCTION dbo.passenger_exceeds_baggage_limit(
    @purchase_order_id INT
)
RETURNS TABLE
AS
RETURN (
    WITH internal_legs AS (
        SELECT
            r.weight_limit_carry_on_baggage AS WeightLimitCarryOnBaggage,
            r.weight_limit_checked_baggage  AS WeightLimitCheckedBaggage
        FROM dbo.purchaseOrder_flight pf
        JOIN dbo.flight_internal fi ON fi.flight_guid = pf.FlightGuid
        JOIN dbo.[route] r          ON r.id           = fi.route_id
            AND r.is_deleted = 0
        WHERE pf.PurchaseOrderId = @purchase_order_id
    ),
    most_restrictive AS (
        SELECT
            MIN(WeightLimitCarryOnBaggage) AS MaxCarryOnWeight,
            MIN(WeightLimitCheckedBaggage) AS MaxCheckedWeight
        FROM internal_legs
    )
    SELECT
        p.Id                          AS PassengerId,
        p.CarryOnLuggage * 7          AS CarryOnWeight,
        p.CheckedLuggage * 23         AS CheckedWeight,
        mr.MaxCarryOnWeight           AS MaxCarryOnWeight,
        mr.MaxCheckedWeight           AS MaxCheckedWeight,
        CASE WHEN p.CarryOnLuggage * 7  > mr.MaxCarryOnWeight THEN 1 ELSE 0 END AS ExceedsCarryOn,
        CASE WHEN p.CheckedLuggage * 23 > mr.MaxCheckedWeight THEN 1 ELSE 0 END AS ExceedsChecked
    FROM dbo.Passenger p
    CROSS JOIN most_restrictive mr
    WHERE p.PurchaseOrderId = @purchase_order_id
);
GO