CREATE TABLE city (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    country_id INT NOT NULL,
    
    CONSTRAINT fk_city_country 
    FOREIGN KEY (country_id) 
    REFERENCES country(id)
);
