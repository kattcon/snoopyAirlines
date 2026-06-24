CREATE OR ALTER FUNCTION [dbo].[GetPassengersByConfirmation](
@confirmation_code VARCHAR(12)
)
RETURNS TABLE
AS
RETURN(
	SELECT
		p.firstName AS FirstName,
		p.LastName AS LastName,
		p.CheckedLuggage AS CheckedLuggage

	FROM dbo.booking b
	JOIN dbo.Passenger p ON p.PurchaseOrderId = b.purchase_order_id

	WHERE b.confirmation_code = UPPER(LTRIM(RTRIM(@confirmation_code)))
);