CREATE TABLE airport (
    id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    name VARCHAR(200) NOT NULL,
    code VARCHAR(3) NOT NULL UNIQUE,
    city_id INT NOT NULL,
    timezone VARCHAR(100) NOT NULL,
    
    CONSTRAINT fk_airport_city 
    FOREIGN KEY (city_id) 
    REFERENCES city(id)
);