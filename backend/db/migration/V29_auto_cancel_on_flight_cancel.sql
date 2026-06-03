
CREATE TRIGGER dbo.trg_flight_auto_cancel_bookings
ON dbo.flight
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE b
    SET b.status = 'cancelled'
    FROM dbo.booking b
    INNER JOIN dbo.itinerary it ON it.booking_guid = b.guid
    INNER JOIN dbo.flight f ON f.guid = it.flight_guid
    INNER JOIN inserted i ON i.guid = f.guid
    INNER JOIN deleted d ON d.guid = f.guid
    WHERE i.status = 'cancelled'
      AND ISNULL(d.status, '') <> 'cancelled'
      AND b.status = 'confirmed';
END;
GO
