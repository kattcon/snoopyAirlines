-- V13_adding-airport-data.sql

WITH AirportRecord (airport_name, airport_code, city_name, country_name) AS (
    SELECT *
    FROM (VALUES
        ('Juan Santamaria International Airport', 'SJO', 'San Jose', 'Costa Rica'),
        ('Daniel Oduber Quiros International Airport', 'LIR', 'Liberia', 'Costa Rica'),
        ('John F. Kennedy International Airport', 'JFK', 'New York', 'United States'),
        ('Los Angeles International Airport', 'LAX', 'Los Angeles', 'United States'),
        ('Miami International Airport', 'MIA', 'Miami', 'United States'),
        ('Toronto Pearson International Airport', 'YYZ', 'Toronto', 'Canada'),
        ('London Heathrow Airport', 'LHR', 'London', 'United Kingdom'),
        ('Madrid-Barajas Airport', 'MAD', 'Madrid', 'Spain'),
        ('Frankfurt Airport', 'FRA', 'Frankfurt', 'Germany'),
        ('Amsterdam Schiphol Airport', 'AMS', 'Amsterdam', 'Netherlands')
    ) AS records (airport_name, airport_code, city_name, country_name)
)
INSERT INTO airport (name, code, city_id)
SELECT
    airport.airport_name,
    airport.airport_code,
    city.id
FROM AirportRecord airport
INNER JOIN country ON country.name = airport.country_name
INNER JOIN city ON city.name = airport.city_name
    AND city.country_id = country.id;
