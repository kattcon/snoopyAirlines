CREATE TABLE PurchaseOrder (
    Id        INT PRIMARY KEY IDENTITY(1,1),
    FlightId  INT          NOT NULL,
    SeatClass VARCHAR(50)  NOT NULL,
    Status    VARCHAR(20)  NOT NULL DEFAULT 'pending'
);

CREATE TABLE Passenger (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    PurchaseOrderId INT          NOT NULL REFERENCES PurchaseOrder(Id),
    Gender          VARCHAR(20)  NOT NULL,
    FirstName       VARCHAR(100) NOT NULL,
    LastName        VARCHAR(100) NOT NULL,
    BirthDay        VARCHAR(2)   NOT NULL,
    BirthMonth      VARCHAR(10)   NOT NULL,
    BirthYear       VARCHAR(4)   NOT NULL,
    Nationality     VARCHAR(100) NOT NULL
);
