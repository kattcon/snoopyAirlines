-- V12_adding-airport-data.sql

-- Costa Rica airports
INSERT INTO airport (name, code, city_id, timezone) VALUES
('Juan Santamaría International Airport', 'SJO', 91, 'America/Costa_Rica'),
('Daniel Oduber Quirós International Airport', 'LIR', 92, 'America/Costa_Rica');

-- International airports
INSERT INTO airport (name, code, city_id, timezone) VALUES
('John F. Kennedy International Airport', 'JFK', 343, 'America/New_York'), -- New York
('Los Angeles International Airport', 'LAX', 344, 'America/Los_Angeles'), -- Los Angeles
('Miami International Airport', 'MIA', 346, 'America/New_York'), -- Miami
('Toronto Pearson International Airport', 'YYZ', 61, 'America/Toronto'), -- Toronto
('London Heathrow Airport', 'LHR', 337, 'Europe/London'), -- London
('Madrid-Barajas Airport', 'MAD', 293, 'Europe/Madrid'), -- Madrid
('Frankfurt Airport', 'FRA', 134, 'Europe/Berlin'), -- Frankfurt
('Amsterdam Schiphol Airport', 'AMS', 229, 'Europe/Amsterdam'); -- Amsterdam