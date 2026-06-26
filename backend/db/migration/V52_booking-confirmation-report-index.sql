
CREATE INDEX IX_booking_status_confirmed_at
ON dbo.booking (status, confirmed_at)
INCLUDE (purchase_order_id);

CREATE INDEX IX_Passenger_PurchaseOrderId
ON dbo.Passenger (PurchaseOrderId);