CREATE PROCEDURE sp_delete_route
    @RouteId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [route] WHERE id = @RouteId)
    BEGIN
        RETURN; -- no existe
    END

    IF EXISTS (SELECT 1 FROM flight_internal WHERE route_id = @RouteId)
    BEGIN
        -- Soft delete
        UPDATE [route] SET is_deleted = 1 WHERE id = @RouteId;
    END
    ELSE
    BEGIN
        -- Hard delete
        DELETE FROM [route] WHERE id = @RouteId;
    END
END;
GO