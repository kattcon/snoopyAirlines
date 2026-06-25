CREATE TABLE dbo.invoice (
    id INT NOT NULL IDENTITY(1, 1) CONSTRAINT pk_invoice PRIMARY KEY,
    booking_guid UNIQUEIDENTIFIER NOT NULL,
    created_at DATETIME2(0) NOT NULL CONSTRAINT df_invoice_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_invoice_booking FOREIGN KEY (booking_guid) REFERENCES dbo.booking (guid),
    CONSTRAINT uq_invoice_booking UNIQUE (booking_guid)
);
GO

CREATE TABLE dbo.invoice_flight (
    id INT NOT NULL IDENTITY(1, 1) CONSTRAINT pk_invoice_flight PRIMARY KEY,
    invoice_id INT NOT NULL,
    passenger_id INT NOT NULL,
    flight_guid UNIQUEIDENTIFIER NOT NULL,
    seat_class VARCHAR(20) NOT NULL,
    unit_price DECIMAL(10, 2) NOT NULL,

    CONSTRAINT fk_invoice_flight_invoice FOREIGN KEY (invoice_id) REFERENCES dbo.invoice (id),
    CONSTRAINT fk_invoice_flight_passenger FOREIGN KEY (passenger_id) REFERENCES dbo.Passenger (Id),
    CONSTRAINT fk_invoice_flight_flight FOREIGN KEY (flight_guid) REFERENCES dbo.flight (guid),
    CONSTRAINT ck_invoice_flight_unit_price CHECK (unit_price >= 0)
);
GO

CREATE INDEX ix_invoice_flight_invoice_flight ON dbo.invoice_flight (invoice_id, flight_guid);
GO

CREATE TABLE dbo.invoice_luggage (
    id INT NOT NULL IDENTITY(1, 1) CONSTRAINT pk_invoice_luggage PRIMARY KEY,
    invoice_id INT NOT NULL,
    passenger_id INT NOT NULL,
    carry_on_quantity INT NOT NULL,
    carry_on_unit_price DECIMAL(10, 2) NOT NULL,
    carry_on_total DECIMAL(10, 2) NOT NULL,
    checked_quantity INT NOT NULL,
    checked_unit_price DECIMAL(10, 2) NOT NULL,
    checked_multiplier DECIMAL(5, 4) NOT NULL,
    checked_total DECIMAL(10, 2) NOT NULL,

    CONSTRAINT fk_invoice_luggage_invoice FOREIGN KEY (invoice_id) REFERENCES dbo.invoice (id),
    CONSTRAINT fk_invoice_luggage_passenger FOREIGN KEY (passenger_id) REFERENCES dbo.Passenger (Id),
    CONSTRAINT ck_invoice_luggage_carry_on_total CHECK (carry_on_total >= 0),
    CONSTRAINT ck_invoice_luggage_checked_total CHECK (checked_total >= 0)
);
GO

CREATE INDEX ix_invoice_luggage_invoice ON dbo.invoice_luggage (invoice_id);
GO
