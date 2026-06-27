ALTER TABLE [dbo].[booking]
    ADD [cancellation_token]            VARCHAR(64)  NULL,
        [cancellation_token_expires_at] DATETIME2(0) NULL;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CancelBooking]
    @token_hash VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY

        DECLARE @booking_guid UNIQUEIDENTIFIER;

        SELECT @booking_guid = guid
        FROM   dbo.booking
        WHERE  cancellation_token            = @token_hash
          AND  cancellation_token_expires_at > SYSDATETIME()
          AND  [status]                     <> 'Cancelada';

        IF @booking_guid IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success;
            RETURN;
        END;

        UPDATE dbo.booking
        SET    [status]                      = 'Cancelada',
               cancellation_token            = NULL,
               cancellation_token_expires_at = NULL
        WHERE  guid = @booking_guid;

        COMMIT TRANSACTION;
        SELECT 1 AS Success;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO