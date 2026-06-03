CREATE TABLE dbo.flight (
    guid            UNIQUEIDENTIFIER
                    NOT NULL
                    CONSTRAINT df_flight_guid DEFAULT NEWSEQUENTIALID(),
    route_id        INT
                    NOT NULL,
    departure_at    DATETIME2(0)
                    NOT NULL,
    arrival_at      DATETIME2(0)
                    NOT NULL,
    status          VARCHAR(20)
                    NOT NULL
                    CONSTRAINT df_flight_status DEFAULT 'scheduled',
    created_at      DATETIME2(0)
                    NOT NULL
                    CONSTRAINT df_flight_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_flight PRIMARY KEY (guid),
    CONSTRAINT fk_flight_route FOREIGN KEY (route_id) REFERENCES dbo.[route](id),
    CONSTRAINT ck_flight_status CHECK (status IN ('scheduled', 'cancelled', 'completed')),
    CONSTRAINT ck_flight_arrival_after_departure CHECK (arrival_at > departure_at),
    CONSTRAINT uq_flight_route_departure UNIQUE (route_id, departure_at)
);
GO
