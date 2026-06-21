-- Agregar is_deleted a airport
ALTER TABLE dbo.airport
    ADD is_deleted BIT NOT NULL CONSTRAINT df_airport_is_deleted DEFAULT 0;
GO

ALTER TABLE dbo.[route]
    ADD is_deleted BIT NOT NULL CONSTRAINT df_route_is_deleted DEFAULT 0;
GO

DECLARE @fk_departure NVARCHAR(256);
DECLARE @fk_arrival NVARCHAR(256);

SELECT @fk_departure = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.[route]')
  AND c.name = 'departure_airport_id';

SELECT @fk_arrival = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.[route]')
  AND c.name = 'arrival_airport_id';

EXEC('ALTER TABLE dbo.[route] DROP CONSTRAINT ' + @fk_departure);
EXEC('ALTER TABLE dbo.[route] DROP CONSTRAINT ' + @fk_arrival);
GO

ALTER TABLE dbo.[route]
    ADD CONSTRAINT fk_route_departure_airport
        FOREIGN KEY (departure_airport_id) REFERENCES dbo.airport(id) ON DELETE NO ACTION;
GO

ALTER TABLE dbo.[route]
    ADD CONSTRAINT fk_route_arrival_airport
        FOREIGN KEY (arrival_airport_id) REFERENCES dbo.airport(id) ON DELETE NO ACTION;
GO

CREATE TRIGGER dbo.trg_airport_hard_delete
ON dbo.airport
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DELETE dbo.itinerary
    FROM dbo.itinerary it
    INNER JOIN dbo.flight f ON f.guid = it.flight_guid
    INNER JOIN dbo.[route] r ON r.id = f.route_id
    WHERE r.departure_airport_id IN (SELECT id FROM deleted)
       OR r.arrival_airport_id IN (SELECT id FROM deleted);

    DELETE dbo.flight
    FROM dbo.flight f
    INNER JOIN dbo.[route] r ON r.id = f.route_id
    WHERE r.departure_airport_id IN (SELECT id FROM deleted)
       OR r.arrival_airport_id IN (SELECT id FROM deleted);

    DELETE FROM dbo.[route]
    WHERE departure_airport_id IN (SELECT id FROM deleted)
       OR arrival_airport_id IN (SELECT id FROM deleted);

    DELETE FROM dbo.airport
    WHERE id IN (SELECT id FROM deleted);
END;
GO
