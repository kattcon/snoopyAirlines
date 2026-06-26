EXEC sp_rename N'dbo.PurchaseOrderRoute', N'purchaseOrder_flight';
GO

EXEC sp_rename N'dbo.pk_purchase_order_route', N'pk_purchaseOrder_flight', N'OBJECT';
GO

EXEC sp_rename N'dbo.ck_purchase_order_route_sequence', N'ck_purchaseOrder_flight_sequence', N'OBJECT';
GO

EXEC sp_rename N'dbo.fk_purchase_order_route_purchase_order', N'fk_purchaseOrder_flight_purchaseOrder', N'OBJECT';
GO

ALTER TABLE dbo.purchaseOrder_flight
    ADD FlightGuid UNIQUEIDENTIFIER NULL;
GO

UPDATE por
SET FlightGuid = itinerary.flight_guid
FROM dbo.purchaseOrder_flight por
INNER JOIN dbo.booking booking
    ON booking.purchase_order_id = por.PurchaseOrderId
INNER JOIN dbo.itinerary itinerary
    ON itinerary.booking_guid = booking.guid
    AND itinerary.sequence_number = por.SequenceNumber;
GO

DELETE passenger
FROM dbo.Passenger passenger
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.booking booking
    WHERE booking.purchase_order_id = passenger.PurchaseOrderId
);
GO

DELETE por
FROM dbo.purchaseOrder_flight por
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.booking booking
    WHERE booking.purchase_order_id = por.PurchaseOrderId
);
GO

DELETE purchase_order
FROM dbo.PurchaseOrder purchase_order
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.booking booking
    WHERE booking.purchase_order_id = purchase_order.Id
);
GO

IF EXISTS (
    SELECT 1
    FROM dbo.purchaseOrder_flight
    WHERE FlightGuid IS NULL
)
BEGIN
    THROW 50000, 'Could not resolve every booked purchase order leg to a flight.', 1;
END;
GO

DROP INDEX ix_purchase_order_route_route
    ON dbo.purchaseOrder_flight;
GO

ALTER TABLE dbo.purchaseOrder_flight
    DROP CONSTRAINT fk_purchase_order_route_route;
GO

ALTER TABLE dbo.purchaseOrder_flight
    ALTER COLUMN FlightGuid UNIQUEIDENTIFIER NOT NULL;
GO

ALTER TABLE dbo.purchaseOrder_flight
    ADD CONSTRAINT fk_purchaseOrder_flight_flight
        FOREIGN KEY (FlightGuid) REFERENCES dbo.flight(guid);
GO

CREATE INDEX ix_purchaseOrder_flight_flight
    ON dbo.purchaseOrder_flight (FlightGuid);
GO

ALTER TABLE dbo.purchaseOrder_flight
    DROP COLUMN RouteId, IntendedDate;
GO
