CREATE FUNCTION dbo.remaining_seats (
    @flight_guid UNIQUEIDENTIFIER
)
RETURNS INT
AS
BEGIN
    DECLARE @capacity INT;
    DECLARE @booked_seats INT;

    SELECT
        @capacity =
            (airplane.tourist_rows * airplane.tourist_columns)
            + (airplane.firstClass_rows * airplane.firstClass_columns)
    FROM 
        dbo.flight
        INNER JOIN dbo.[route] ON [route].id = flight.route_id
        INNER JOIN dbo.airplane ON airplane.id = [route].airplane_id
    WHERE
        flight.guid = @flight_guid;

    IF @capacity IS NULL
    BEGIN
        RETURN NULL;
    END;

    SELECT
        @booked_seats = COUNT(passenger.Id)
    FROM
        dbo.booking
        INNER JOIN dbo.PurchaseOrder ON PurchaseOrder.Id = booking.purchase_order_id
        INNER JOIN dbo.Passenger ON Passenger.PurchaseOrderId = PurchaseOrder.Id
    WHERE 
        booking.flight_guid = @flight_guid;

    RETURN @capacity - COALESCE(@booked_seats, 0);
END;
GO