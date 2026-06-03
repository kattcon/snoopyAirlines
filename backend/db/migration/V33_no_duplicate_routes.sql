GO
CREATE TRIGGER TR_No_duplicate_routes
ON dbo.route
AFTER INSERT
AS
BEGIN
	IF EXISTS(
		SELECT 1
		FROM inserted i
		JOIN dbo.route r ON r.departure_airport_id = i.departure_airport_id
			AND r.arrival_airport_id = i.arrival_airport_id
			AND r.departure_time = i.departure_time
			AND r.arrival_time = i.arrival_time
			AND r.frequency = i.frequency
			AND r.id <> i.id
	)
	BEGIN
		RAISERROR('Ya existe una ruta con los datos proporcionados', 16, 1);
		ROLLBACK TRANSACTION;
	END
END;