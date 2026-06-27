CREATE TABLE dbo.flight_internal (
    flight_guid UNIQUEIDENTIFIER NOT NULL,
    route_id    INT              NOT NULL,

    CONSTRAINT pk_flight_internal PRIMARY KEY (flight_guid),
    CONSTRAINT fk_flight_internal_flight FOREIGN KEY (flight_guid)
        REFERENCES dbo.flight(guid) ON DELETE CASCADE,
    CONSTRAINT fk_flight_internal_route FOREIGN KEY (route_id)
        REFERENCES dbo.[route](id)
);
GO

CREATE INDEX ix_flight_internal_route
    ON dbo.flight_internal (route_id);
GO

INSERT INTO dbo.flight_internal (
    flight_guid,
    route_id
)
SELECT
    guid,
    route_id
FROM dbo.flight;
GO

CREATE TABLE dbo.flight_external (
    flight_guid            UNIQUEIDENTIFIER NOT NULL,
    partner_airline_id     INT              NOT NULL,
    departure_airport_code CHAR(3)          NOT NULL,
    departure_airport_name VARCHAR(120)     NOT NULL,
    departure_city         VARCHAR(120)     NOT NULL,
    arrival_airport_code   CHAR(3)          NOT NULL,
    arrival_airport_name   VARCHAR(120)     NOT NULL,
    arrival_city           VARCHAR(120)     NOT NULL,
    tourist_price          DECIMAL(10, 2)   NOT NULL,
    first_class_price      DECIMAL(10, 2)   NOT NULL,
    carry_on_price         DECIMAL(10, 2)   NOT NULL,
    checked_price          DECIMAL(10, 2)   NOT NULL,

    CONSTRAINT pk_flight_external PRIMARY KEY (flight_guid),
    CONSTRAINT fk_flight_external_flight FOREIGN KEY (flight_guid)
        REFERENCES dbo.flight(guid) ON DELETE CASCADE,
    CONSTRAINT fk_flight_external_partner_airline FOREIGN KEY (partner_airline_id)
        REFERENCES dbo.partner_airline(id),
    CONSTRAINT ck_flight_external_airport_codes CHECK (
        departure_airport_code LIKE '[A-Z][A-Z][A-Z]'
        AND arrival_airport_code LIKE '[A-Z][A-Z][A-Z]'
    ),
    CONSTRAINT ck_flight_external_prices CHECK (
        tourist_price >= 0
        AND first_class_price >= 0
        AND carry_on_price >= 0
        AND checked_price >= 0
    )
);
GO

CREATE INDEX ix_flight_external_partner_airline
    ON dbo.flight_external (partner_airline_id);
GO

CREATE OR ALTER TRIGGER dbo.trg_flight_internal_validate
ON dbo.flight_internal
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.flight_external external_flight
            ON external_flight.flight_guid = i.flight_guid
    )
    BEGIN
        THROW 50000, 'A flight cannot be both internal and external.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.flight f ON f.guid = i.flight_guid
        INNER JOIN dbo.flight_internal existing_internal
            ON existing_internal.route_id = i.route_id
            AND existing_internal.flight_guid <> i.flight_guid
        INNER JOIN dbo.flight existing_flight
            ON existing_flight.guid = existing_internal.flight_guid
            AND existing_flight.departure_at = f.departure_at
    )
    BEGIN
        THROW 50000, 'An internal flight already exists for this route and departure time.', 1;
    END;
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_flight_external_validate
ON dbo.flight_external
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.flight_internal internal_flight
            ON internal_flight.flight_guid = i.flight_guid
    )
    BEGIN
        THROW 50000, 'A flight cannot be both internal and external.', 1;
    END;
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_flight_validate_internal_uniqueness
ON dbo.flight
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(departure_at)
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM inserted i
            INNER JOIN dbo.flight_internal internal_flight
                ON internal_flight.flight_guid = i.guid
            INNER JOIN dbo.flight_internal existing_internal
                ON existing_internal.route_id = internal_flight.route_id
                AND existing_internal.flight_guid <> i.guid
            INNER JOIN dbo.flight existing_flight
                ON existing_flight.guid = existing_internal.flight_guid
                AND existing_flight.departure_at = i.departure_at
        )
        BEGIN
            THROW 50000, 'An internal flight already exists for this route and departure time.', 1;
        END;
    END;
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_flight_internal_delete_parent
ON dbo.flight_internal
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DELETE f
    FROM dbo.flight f
    INNER JOIN deleted d ON d.flight_guid = f.guid
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.flight_internal internal_flight
        WHERE internal_flight.flight_guid = f.guid
    )
      AND NOT EXISTS (
        SELECT 1
        FROM dbo.flight_external external_flight
        WHERE external_flight.flight_guid = f.guid
    );
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_flight_external_delete_parent
ON dbo.flight_external
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DELETE f
    FROM dbo.flight f
    INNER JOIN deleted d ON d.flight_guid = f.guid
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.flight_internal internal_flight
        WHERE internal_flight.flight_guid = f.guid
    )
      AND NOT EXISTS (
        SELECT 1
        FROM dbo.flight_external external_flight
        WHERE external_flight.flight_guid = f.guid
    );
END;
GO

ALTER TABLE dbo.flight DROP CONSTRAINT uq_flight_route_departure;
GO

ALTER TABLE dbo.flight DROP CONSTRAINT fk_flight_route;
GO

ALTER TABLE dbo.flight DROP COLUMN route_id;
GO
