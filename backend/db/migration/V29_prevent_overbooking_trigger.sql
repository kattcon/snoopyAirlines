CREATE TRIGGER dbo.trg_booking_prevent_overbooking
ON dbo.booking
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM inserted WHERE flight_guid IS NOT NULL)
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM (SELECT DISTINCT flight_guid FROM inserted WHERE flight_guid IS NOT NULL) i
        LEFT JOIN dbo.flight f ON f.guid = i.flight_guid
        LEFT JOIN dbo.route r ON r.id = f.route_id
        LEFT JOIN dbo.airplane a ON a.id = r.airplane_id
        WHERE a.id IS NULL
    )
    BEGIN
        RAISERROR('Vuelo o aeronave no encontrados al validar disponibilidad.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM (SELECT DISTINCT flight_guid FROM inserted WHERE flight_guid IS NOT NULL) i
        JOIN dbo.flight f    ON f.guid = i.flight_guid
        JOIN dbo.route r     ON r.id   = f.route_id
        JOIN dbo.airplane a  ON a.id   = r.airplane_id
        JOIN dbo.booking b   ON b.flight_guid = f.guid
                             AND b.status = 'confirmed'
        JOIN dbo.Passenger p ON p.PurchaseOrderId = b.purchase_order_id
        GROUP BY f.guid,
                 a.tourist_rows, a.tourist_columns,
                 a.firstClass_rows, a.firstClass_columns
        HAVING COUNT(p.Id) > (a.tourist_rows * a.tourist_columns)
                            + (a.firstClass_rows * a.firstClass_columns)
    )
    BEGIN
        RAISERROR('No hay asientos disponibles para el vuelo.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO