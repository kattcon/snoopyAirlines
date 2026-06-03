SET XACT_ABORT ON;

DELETE FROM dbo.itinerary;
DELETE FROM dbo.booking;
DELETE FROM dbo.flight;
DELETE FROM dbo.Passenger;
DELETE FROM dbo.PurchaseOrderRoute;
DELETE FROM dbo.PurchaseOrder;
DELETE FROM dbo.[route];
DELETE FROM dbo.airplane;


DECLARE @next_seed INT;

SELECT @next_seed = CASE WHEN last_value IS NULL THEN 1 ELSE 0 END
FROM sys.identity_columns
WHERE object_id = OBJECT_ID(N'dbo.Passenger');
DBCC CHECKIDENT (N'dbo.Passenger', RESEED, @next_seed);

SELECT @next_seed = CASE WHEN last_value IS NULL THEN 1 ELSE 0 END
FROM sys.identity_columns
WHERE object_id = OBJECT_ID(N'dbo.PurchaseOrder');
DBCC CHECKIDENT (N'dbo.PurchaseOrder', RESEED, @next_seed);

SELECT @next_seed = CASE WHEN last_value IS NULL THEN 1 ELSE 0 END
FROM sys.identity_columns
WHERE object_id = OBJECT_ID(N'dbo.airplane');
DBCC CHECKIDENT (N'dbo.airplane', RESEED, @next_seed);

SELECT @next_seed = CASE WHEN last_value IS NULL THEN 1 ELSE 0 END
FROM sys.identity_columns
WHERE object_id = OBJECT_ID(N'dbo.[route]');
DBCC CHECKIDENT (N'dbo.[route]', RESEED, @next_seed);

-- Existing V15 seed already includes SJO, MAD, FRA, and AMS. Add the route-required
-- airport codes that are missing from the existing seed before inserting routes.
WITH MissingAirportSeed (airport_name, airport_code, city_name, country_name) AS (
    SELECT *
    FROM (VALUES
        ('Hartsfield-Jackson Atlanta International Airport', 'ATL', 'Atlanta', 'United States'),
        ('Melbourne Airport', 'MEL', 'Melbourne', 'Australia'),
        ('Minsk National Airport', 'MSQ', 'Minsk', 'Belarus'),
        ('Paris Charles de Gaulle Airport', 'CDG', 'Paris', 'France')
    ) AS records (airport_name, airport_code, city_name, country_name)
)
INSERT INTO dbo.airport (name, code, city_id)
SELECT
    MissingAirportSeed.airport_name,
    MissingAirportSeed.airport_code,
    city.id
FROM MissingAirportSeed
INNER JOIN dbo.country
    ON country.name = MissingAirportSeed.country_name
INNER JOIN dbo.city
    ON city.name = MissingAirportSeed.city_name
    AND city.country_id = country.id
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.airport
    WHERE airport.code = MissingAirportSeed.airport_code
);

INSERT INTO dbo.airplane (
    model,
    tourist_rows,
    tourist_columns,
    firstClass_rows,
    firstClass_columns,
    max_weight
)
VALUES
    ('PI-Sprint2', 27, 6, 3, 4, 3000),
    ('AIRBUS-2026', 20, 4, 2, 2, 3000);

INSERT INTO dbo.[route] (
    airplane_id,
    departure_airport_id,
    arrival_airport_id,
    departure_time,
    arrival_time,
    frequency,
    duration_minutes,
    price_first_class,
    price_economy_class,
    price_carry_on_baggage,
    price_checked_baggage,
    weight_limit_carry_on_baggage,
    weight_limit_checked_baggage,
    checked_baggage_price_multiplier
)
SELECT
    airplane.id,
    departure_airport.id,
    arrival_airport.id,
    RouteSeed.departure_time,
    RouteSeed.arrival_time,
    RouteSeed.frequency,
    RouteSeed.duration_minutes,
    RouteSeed.price_first_class,
    RouteSeed.price_economy_class,
    RouteSeed.price_carry_on_baggage,
    RouteSeed.price_checked_baggage,
    RouteSeed.weight_limit_carry_on_baggage,
    RouteSeed.weight_limit_checked_baggage,
    RouteSeed.checked_baggage_price_multiplier
FROM 
    (VALUES
        (1, 'PI-Sprint2', 'SJO', 'MAD', CONVERT(time(0), '07:00:00'), CONVERT(time(0), '23:00:00'), 72, 540, 2000.00, 800.00, 0.00, 100.00, 10, 23, 0.5000),
        (2, 'PI-Sprint2', 'SJO', 'ATL', CONVERT(time(0), '08:00:00'), CONVERT(time(0), '14:00:00'), 9, 300, 1000.00, 400.00, 0.00, 75.00, 10, 23, 0.5000),
        (3, 'PI-Sprint2', 'ATL', 'MAD', CONVERT(time(0), '11:00:00'), CONVERT(time(0), '23:00:00'), 74, 420, 1050.00, 450.00, 0.00, 75.00, 10, 23, 0.5000),
        (4, 'PI-Sprint2', 'ATL', 'MAD', CONVERT(time(0), '17:00:00'), CONVERT(time(0), '05:00:00'), 74, 420, 800.00, 350.00, 0.00, 75.00, 10, 23, 0.5000),
        (5, 'PI-Sprint2', 'MAD', 'MEL', CONVERT(time(0), '05:00:00'), CONVERT(time(0), '08:00:00'), 4, 180, 1000.00, 300.00, 0.00, 50.00, 10, 23, 0.5000),
        (6, 'AIRBUS-2026', 'MSQ', 'SJO', CONVERT(time(0), '01:00:00'), CONVERT(time(0), '04:00:00'), 8, 600, 2200.00, 950.00, 0.00, 100.00, 10, 23, 0.5000),
        (7, 'PI-Sprint2', 'SJO', 'CDG', CONVERT(time(0), '06:30:00'), CONVERT(time(0), '22:30:00'), 72, 540, 1999.00, 799.00, 0.00, 100.00, 10, 23, 0.5000),
        (8, 'PI-Sprint2', 'SJO', 'FRA', CONVERT(time(0), '06:45:00'), CONVERT(time(0), '22:45:00'), 72, 540, 1950.00, 790.00, 0.00, 100.00, 10, 23, 0.5000),
        (9, 'PI-Sprint2', 'SJO', 'AMS', CONVERT(time(0), '07:15:00'), CONVERT(time(0), '23:15:00'), 72, 540, 2050.00, 810.00, 0.00, 100.00, 10, 23, 0.5000)
    ) AS RouteSeed (
        route_sequence,
        airplane_model,
        departure_airport_code,
        arrival_airport_code,
        departure_time,
        arrival_time,
        frequency,
        duration_minutes,
        price_first_class,
        price_economy_class,
        price_carry_on_baggage,
        price_checked_baggage,
        weight_limit_carry_on_baggage,
        weight_limit_checked_baggage,
        checked_baggage_price_multiplier
    )
    INNER JOIN dbo.airplane ON airplane.model = RouteSeed.airplane_model
    INNER JOIN dbo.airport AS departure_airport ON departure_airport.code = RouteSeed.departure_airport_code
    INNER JOIN dbo.airport AS arrival_airport ON arrival_airport.code = RouteSeed.arrival_airport_code
ORDER BY RouteSeed.route_sequence;
