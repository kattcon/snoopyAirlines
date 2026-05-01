-- Works only if the airport table is empty.

ALTER TABLE airport ADD name VARCHAR(200) NOT NULL;
ALTER TABLE airport ADD code VARCHAR(3) NOT NULL;
ALTER TABLE airport ADD city_id INT NOT NULL;
ALTER TABLE airport ADD timezone VARCHAR(100) NOT NULL;

ALTER TABLE airport ADD CONSTRAINT uq_airport_code UNIQUE (code);
ALTER TABLE airport ADD CONSTRAINT fk_airport_city FOREIGN KEY (city_id) REFERENCES city(id);