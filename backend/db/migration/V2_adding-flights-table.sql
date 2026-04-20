CREATE TABLE flight (
    id INT IDENTITY(1,1) PRIMARY KEY,
    airplane_id INT NOT NULL,
    departure_airport_id INT NOT NULL,
    arrival_airport_id INT NOT NULL,
    departure_time DATETIME NOT NULL,
    arrival_time DATETIME NOT NULL,
    duration_minutes INT NOT NULL,
    price_first_class DECIMAL(10, 2) NOT NULL,
    price_economy_class DECIMAL(10, 2) NOT NULL,
    price_carry_on_baggage DECIMAL(10, 2) NOT NULL,
    price_checked_baggage DECIMAL(10, 2) NOT NULL,
    weight_limit_carry_on_baggage INT NOT NULL,
    weight_limit_checked_baggage INT NOT NULL,
    checked_baggage_price_multiplier DECIMAL(5, 4) NOT NULL,
    FOREIGN KEY (airplane_id) REFERENCES airplane(id),
    FOREIGN KEY (departure_airport_id) REFERENCES airport(id),
    FOREIGN KEY (arrival_airport_id) REFERENCES airport(id)
);