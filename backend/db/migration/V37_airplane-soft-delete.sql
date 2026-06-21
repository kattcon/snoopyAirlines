ALTER TABLE dbo.airplane
    ADD is_deleted BIT NOT NULL CONSTRAINT df_airplane_is_deleted DEFAULT 0;
GO

DECLARE @fk_airplane NVARCHAR(256);

SELECT @fk_airplane = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.[route]')
  AND c.name = 'airplane_id';

EXEC('ALTER TABLE dbo.[route] DROP CONSTRAINT ' + @fk_airplane);
GO

ALTER TABLE dbo.[route]
    ADD CONSTRAINT fk_route_airplane
        FOREIGN KEY (airplane_id) REFERENCES dbo.airplane(id) ON DELETE CASCADE;
GO

ALTER TABLE dbo.flight DROP CONSTRAINT fk_flight_route;
GO

ALTER TABLE dbo.flight
    ADD CONSTRAINT fk_flight_route
        FOREIGN KEY (route_id) REFERENCES dbo.[route](id) ON DELETE CASCADE;
GO

ALTER TABLE dbo.itinerary DROP CONSTRAINT fk_itinerary_flight;
GO

ALTER TABLE dbo.itinerary
    ADD CONSTRAINT fk_itinerary_flight
        FOREIGN KEY (flight_guid) REFERENCES dbo.flight(guid) ON DELETE CASCADE;
GO
