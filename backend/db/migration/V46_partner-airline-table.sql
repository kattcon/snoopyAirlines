CREATE TABLE dbo.partner_airline (
    id          INT             NOT NULL IDENTITY(1,1),
    name        VARCHAR(120)    NOT NULL,
    host        VARCHAR(2048)   NOT NULL,
    api_key     VARCHAR(512)    NOT NULL,

    CONSTRAINT pk_partner_airline PRIMARY KEY (id),
    CONSTRAINT uq_partner_airline_name UNIQUE (name),
    CONSTRAINT ck_partner_airline_name CHECK (NULLIF(LTRIM(RTRIM(name)), '') IS NOT NULL),
    CONSTRAINT ck_partner_airline_host CHECK (NULLIF(LTRIM(RTRIM(host)), '') IS NOT NULL),
    CONSTRAINT ck_partner_airline_api_key CHECK (NULLIF(LTRIM(RTRIM(api_key)), '') IS NOT NULL)
);
GO