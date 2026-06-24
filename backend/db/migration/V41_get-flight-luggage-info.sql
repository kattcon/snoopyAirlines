CREATE OR ALTER FUNCTION [dbo].[GetBaggageInfoByConfirmation](
    @confirmation_code VARCHAR(12)
)
RETURNS TABLE
AS
RETURN (
    SELECT
        r.price_checked_baggage             AS PriceCheckedBaggage,
        r.checked_baggage_price_multiplier  AS CheckedBaggagePriceMultiplier

    FROM dbo.booking b
    JOIN dbo.PurchaseOrder po  ON po.Id          = b.purchase_order_id
    JOIN dbo.itinerary i       ON i.booking_guid = b.guid
    JOIN dbo.flight f          ON f.guid         = i.flight_guid
    JOIN dbo.[route] r         ON r.id           = f.route_id
    WHERE
        b.confirmation_code = UPPER(LTRIM(RTRIM(@confirmation_code)))
);
GO