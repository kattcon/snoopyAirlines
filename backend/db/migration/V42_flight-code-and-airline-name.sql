ALTER TABLE dbo.[route]
ADD flight_code VARCHAR(20) NULL;
GO

-- Índice filtrado: garantiza unicidad solo cuando hay un código asignado,
-- permitiendo que múltiples rutas queden sin código (NULL).
CREATE UNIQUE INDEX uq_route_flight_code
    ON dbo.[route] (flight_code)
    WHERE flight_code IS NOT NULL;
GO

ALTER TABLE dbo.booking
ADD airline_name VARCHAR(100) NOT NULL
    CONSTRAINT df_booking_airline_name DEFAULT 'Snoopy Airlines';
GO
