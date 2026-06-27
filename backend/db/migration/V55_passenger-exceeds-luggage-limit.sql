CREATE OR ALTER FUNCTION dbo.passenger_exceeds_baggage_limit(
    @purchase_order_id INT
)
RETURNS TABLE
AS
RETURN (
    SELECT
        p.Id                        AS PassengerId,
        p.CarryOnLuggage * 7             AS CarryOnWeight,
        p.CheckedLuggage  * 23            AS CheckedWeight,
        r.weight_limit_carry_on_baggage      AS MaxCarryOnWeight,
        r.weight_limit_checked_baggage       AS MaxCheckedWeight,
        CASE WHEN p.CarryOnLuggage * 7  > r.weight_limit_carry_on_baggage THEN 1 ELSE 0 END AS ExceedsCarryOn,
        CASE WHEN p.CheckedLuggage  * 23 > r.weight_limit_checked_baggage  THEN 1 ELSE 0 END AS ExceedsChecked
    FROM dbo.Passenger p
    JOIN dbo.PurchaseOrderRoute po ON po.PurchaseOrderId = p.PurchaseOrderId
    JOIN dbo.[route] r        ON r.id  = po.RouteId
    WHERE p.PurchaseOrderId = @purchase_order_id
);
GO