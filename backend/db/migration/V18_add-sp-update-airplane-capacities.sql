CREATE PROCEDURE sp_update_airplane_capacities
    @Id INT,
    @TouristRows INT,
    @TouristColumns INT,
    @FirstclassRows INT,
    @FirstclassColumns INT,
    @MaxWeight FLOAT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM airplane WHERE id = @Id)
    BEGIN
        RAISERROR('No se encontró una aeronave con el id proporcionado.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM airplane
        WHERE id = @Id
          AND (
              tourist_rows > @TouristRows
           OR tourist_columns > @TouristColumns
           OR firstClass_rows > @FirstclassRows
           OR firstClass_columns > @FirstclassColumns
           OR max_weight > @MaxWeight
          )
    )
    BEGIN
        RAISERROR('Los valores solo pueden aumentarse, no disminuirse.', 16, 1);
        RETURN;
    END

    UPDATE airplane
    SET tourist_rows = @TouristRows,
        tourist_columns = @TouristColumns,
        firstClass_rows = @FirstclassRows,
        firstClass_columns = @FirstclassColumns,
        max_weight = @MaxWeight
    WHERE id = @Id;
END
