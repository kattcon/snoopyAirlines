CREATE TABLE dbo.[user] (
    id INT IDENTITY(1,1) PRIMARY KEY,
    identification_number VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    first_name VARCHAR(255) NOT NULL,
    last_name_one VARCHAR(255) NOT NULL,
    last_name_two VARCHAR(255),
    type VARCHAR(2) NOT NULL CHECK (type IN ('AD', 'OP')), -- AD: Admin, OP: Operator
    password_hash VARCHAR(255) NOT NULL,
    password_salt VARCHAR(255) NOT NULL
);

CREATE TABLE pending_user (
    id INT IDENTITY(1,1) PRIMARY KEY,
    identification_number VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    first_name VARCHAR(255) NOT NULL,
    last_name_one VARCHAR(255) NOT NULL,
    last_name_two VARCHAR(255),
    type VARCHAR(2) NOT NULL CHECK (type IN ('AD', 'OP')) -- AD: Admin, OP: Operator
);
