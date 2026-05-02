-- V13_adding-flight-data.sql
-- Adds sample flight data to enable flight search functionality

INSERT INTO flight (
    airplane_id,
    departure_airport_id,
    arrival_airport_id,
    departure_time,
    arrival_time,
    duration_minutes,
    price_first_class,
    price_economy_class,
    price_carry_on_baggage,
    price_checked_baggage,
    weight_limit_carry_on_baggage,
    weight_limit_checked_baggage,
    checked_baggage_price_multiplier
) VALUES
-- Flights from Costa Rica (SJO = 1)
(1, 1, 3, '2026-05-15 08:00:00', '2026-05-15 14:30:00', 510, 2500.00, 450.00, 30.00, 50.00, 10, 23, 1.5000), -- SJO to JFK
(1, 1, 4, '2026-05-16 10:00:00', '2026-05-16 13:45:00', 345, 2200.00, 380.00, 30.00, 50.00, 10, 23, 1.5000), -- SJO to LAX
(2, 1, 5, '2026-05-17 14:00:00', '2026-05-17 18:30:00', 270, 1800.00, 320.00, 30.00, 50.00, 10, 23, 1.5000), -- SJO to MIA
(2, 1, 6, '2026-05-18 16:00:00', '2026-05-18 22:15:00', 375, 2000.00, 350.00, 30.00, 50.00, 10, 23, 1.5000), -- SJO to YYZ

-- Flights to Costa Rica (SJO = 1)
(1, 3, 1, '2026-05-20 22:00:00', '2026-05-21 02:30:00', 510, 2500.00, 450.00, 30.00, 50.00, 10, 23, 1.5000), -- JFK to SJO
(1, 4, 1, '2026-05-21 15:00:00', '2026-05-21 22:45:00', 345, 2200.00, 380.00, 30.00, 50.00, 10, 23, 1.5000), -- LAX to SJO
(2, 5, 1, '2026-05-22 19:00:00', '2026-05-22 21:30:00', 270, 1800.00, 320.00, 30.00, 50.00, 10, 23, 1.5000), -- MIA to SJO
(2, 6, 1, '2026-05-23 12:00:00', '2026-05-23 16:15:00', 375, 2000.00, 350.00, 30.00, 50.00, 10, 23, 1.5000), -- YYZ to SJO

-- Domestic flights in Costa Rica (SJO=1, LIR=2)
(3, 1, 2, '2026-05-10 06:00:00', '2026-05-10 07:15:00', 75, 800.00, 120.00, 20.00, 30.00, 8, 20, 1.2000), -- SJO to LIR
(3, 2, 1, '2026-05-11 08:00:00', '2026-05-11 09:15:00', 75, 800.00, 120.00, 20.00, 30.00, 8, 20, 1.2000); -- LIR to SJO

