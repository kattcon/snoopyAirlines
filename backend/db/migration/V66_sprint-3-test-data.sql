SET XACT_ABORT ON;

BEGIN TRANSACTION;

WITH AirportSeed (airport_name, airport_code, city_name, country_name) AS (
    SELECT *
    FROM (VALUES
        ('Carrasco International Airport', 'MVD', 'Montevideo', 'Uruguay'),
        ('Sao Paulo-Guarulhos International Airport', 'GRU', 'Sao Paulo', 'Brazil')
    ) AS records (airport_name, airport_code, city_name, country_name)
)
INSERT INTO dbo.airport (name, code, city_id)
SELECT
    AirportSeed.airport_name,
    AirportSeed.airport_code,
    city.id
FROM AirportSeed
INNER JOIN dbo.country
    ON country.name = AirportSeed.country_name
INNER JOIN dbo.city
    ON city.name = AirportSeed.city_name
    AND city.country_id = country.id
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.airport
    WHERE airport.code = AirportSeed.airport_code
);

WITH AirplaneSeed (model, tourist_rows, tourist_columns, firstClass_rows, firstClass_columns, max_weight) AS (
    SELECT *
    FROM (VALUES
        ('SNP01', 27, 6, 3, 4, 3000),
        ('SNP02', 20, 4, 2, 2, 3000),
        ('SNP03', 20, 4, 2, 2, 3000)
    ) AS records (model, tourist_rows, tourist_columns, firstClass_rows, firstClass_columns, max_weight)
)
INSERT INTO dbo.airplane (
    model,
    tourist_rows,
    tourist_columns,
    firstClass_rows,
    firstClass_columns,
    max_weight
)
SELECT
    AirplaneSeed.model,
    AirplaneSeed.tourist_rows,
    AirplaneSeed.tourist_columns,
    AirplaneSeed.firstClass_rows,
    AirplaneSeed.firstClass_columns,
    AirplaneSeed.max_weight
FROM AirplaneSeed
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.airplane
    WHERE airplane.model = AirplaneSeed.model
);

WITH UserSeed (
    identification_number,
    email,
    first_name,
    last_name_one,
    last_name_two,
    type,
    password_hash,
    password_salt
) AS (
    SELECT *
    FROM (VALUES
        ('660000001', 'sprint3.admin1@snoopyairlines.com', 'Sprint3', 'Admin', 'One', 'AD', '$2a$11$wVAmBSnPwpHtaVrz/NBaAeDcFXYKW0QigNIpnc/0l8w/C6lLxi8m.', ''),
        ('660000002', 'sprint3.admin2@snoopyairlines.com', 'Sprint3', 'Admin', 'Two', 'AD', '$2a$11$wVAmBSnPwpHtaVrz/NBaAeDcFXYKW0QigNIpnc/0l8w/C6lLxi8m.', ''),
        ('660000003', 'sprint3.operator@snoopyairlines.com', 'Sprint3', 'Operator', 'One', 'OP', '$2a$11$wVAmBSnPwpHtaVrz/NBaAeDcFXYKW0QigNIpnc/0l8w/C6lLxi8m.', '')
    ) AS records (
        identification_number,
        email,
        first_name,
        last_name_one,
        last_name_two,
        type,
        password_hash,
        password_salt
    )
)
INSERT INTO dbo.[user] (
    identification_number,
    email,
    first_name,
    last_name_one,
    last_name_two,
    type,
    password_hash,
    password_salt
)
SELECT
    UserSeed.identification_number,
    UserSeed.email,
    UserSeed.first_name,
    UserSeed.last_name_one,
    UserSeed.last_name_two,
    UserSeed.type,
    UserSeed.password_hash,
    UserSeed.password_salt
FROM UserSeed
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.[user]
    WHERE [user].email = UserSeed.email
       OR [user].identification_number = UserSeed.identification_number
);

WITH RouteSeed (
    flight_code,
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
) AS (
    SELECT *
    FROM (VALUES
        ('SN401', 'SNP01', 'MVD', 'GRU', CONVERT(time(0), '11:30:00'), CONVERT(time(0), '14:30:00'), 72, 180, 950.00, 500.00, 0.00, 50.00, 10, 23, 0.5000),
        ('SN402', 'SNP01', 'GRU', 'SJO', CONVERT(time(0), '17:00:00'), CONVERT(time(0), '23:00:00'), 72, 360, 1000.00, 650.00, 0.00, 50.00, 10, 23, 0.5000),
        ('SN403', 'SNP02', 'MVD', 'JFK', CONVERT(time(0), '11:00:00'), CONVERT(time(0), '23:00:00'), 74, 420, 1050.00, 450.00, 0.00, 100.00, 10, 23, 0.5000),
        ('SN404', 'SNP02', 'MVD', 'JFK', CONVERT(time(0), '10:00:00'), CONVERT(time(0), '22:00:00'), 1, 420, 1050.00, 450.00, 0.00, 100.00, 10, 23, 0.5000)
    ) AS records (
        flight_code,
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
)
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
    checked_baggage_price_multiplier,
    flight_code
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
    RouteSeed.checked_baggage_price_multiplier,
    RouteSeed.flight_code
FROM RouteSeed
INNER JOIN dbo.airplane
    ON airplane.model = RouteSeed.airplane_model
INNER JOIN dbo.airport AS departure_airport
    ON departure_airport.code = RouteSeed.departure_airport_code
INNER JOIN dbo.airport AS arrival_airport
    ON arrival_airport.code = RouteSeed.arrival_airport_code
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.[route]
    WHERE [route].flight_code = RouteSeed.flight_code
);

COMMIT TRANSACTION;
