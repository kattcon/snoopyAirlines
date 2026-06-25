-- Agrega código amigable de vuelo a la tabla de rutas (ej. KV503)
-- y nombre de aerolínea a la reserva (por defecto 'Snoopy Airlines').

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
