CREATE TABLE api_user (
    id INT IDENTITY(1,1) PRIMARY KEY,
    api_key VARCHAR(256) NOT NULL UNIQUE,
    name VARCHAR(32) NOT NULL,
    role VARCHAR(2) NOT NULL,

    CONSTRAINT ck_api_user_api_key_length CHECK (DATALENGTH(api_key) = 256),
    CONSTRAINT ck_api_user_role CHECK (role IN ('AD', 'US'))
);

-- Insert a default admin user.
INSERT INTO api_user (api_key, name, role)
VALUES ('u2fdwOUDKCv8X7GhhX9m6ZU2Rak5Z4b2Qi7wV35BqYEnA0wh2rDndiJSuTO3bFVERudTzF9GQwuC0AvXRoWTp3uVQev6ID18yxtby2kiMR3ak0RGvPmFKz1NHQXFTcVNbTIuj60bxVhNeiZQrGem83mVfFRXocAeNfFoO7IM2qJwi27VrV8SfqvtCh62xIlgpquCqRr53KVL02Rvk1s4w9IFAL2Xod1MCjtzyvdnffgXcxMDco4Vw1u1BiZFHSv5', 'admin', 'AD');
