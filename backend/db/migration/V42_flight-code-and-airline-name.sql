ALTER TABLE dbo.[route]
ADD flight_code VARCHAR(20) NULL;
GO

ALTER TABLE dbo.[route]
ADD CONSTRAINT uq_route_flight_code UNIQUE (flight_code);
GO

ALTER TABLE dbo.booking
ADD airline_name VARCHAR(100) NOT NULL
    CONSTRAINT df_booking_airline_name DEFAULT 'Snoopy Airlines';
GO
