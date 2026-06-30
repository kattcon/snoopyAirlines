ALTER TABLE dbo.flight_external
ADD external_flight_uuid VARCHAR(255) NULL;
GO

UPDATE dbo.flight_external
SET external_flight_uuid = CONVERT(VARCHAR(36), flight_guid)
WHERE external_flight_uuid IS NULL;
GO

ALTER TABLE dbo.flight_external
ALTER COLUMN external_flight_uuid VARCHAR(255) NOT NULL;
GO

ALTER TABLE dbo.flight_external
ADD CONSTRAINT ck_flight_external_uuid_not_blank
    CHECK (LEN(LTRIM(RTRIM(external_flight_uuid))) > 0);
GO

CREATE UNIQUE INDEX ux_flight_external_partner_uuid
    ON dbo.flight_external (partner_airline_id, external_flight_uuid);
GO
