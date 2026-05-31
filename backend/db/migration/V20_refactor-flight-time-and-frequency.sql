ALTER TABLE flight
ALTER COLUMN departure_time TIME(0) NOT NULL;
GO

ALTER TABLE flight
ALTER COLUMN arrival_time TIME(0) NOT NULL;
GO

ALTER TABLE flight
ADD frequency TINYINT NOT NULL
    CONSTRAINT df_flight_frequency DEFAULT 127;  -- Every day of the week
GO

ALTER TABLE flight
ADD CONSTRAINT ck_flight_frequency_day_bits
    CHECK (frequency BETWEEN 1 AND 127);
