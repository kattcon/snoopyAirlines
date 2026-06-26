CREATE OR ALTER PROCEDURE [dbo].[UpdateBookingLuggage]
    @ConfirmationCode VARCHAR(12),
    @TotalAmountPaid  DECIMAL(10,2),
    @Passengers       dbo.PassengerLuggageType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY

        IF NOT EXISTS (
            SELECT 1 FROM dbo.booking
            WHERE confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationCode)))
        )
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50001, 'No se encontró ninguna reservación con ese código de confirmación.', 1;
        END

        IF EXISTS (
            SELECT 1 FROM @Passengers p
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.booking b
                JOIN dbo.Passenger pas ON pas.PurchaseOrderId = b.purchase_order_id
                WHERE b.confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationCode)))
                AND pas.Id = p.Id
            )
        )
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50002, 'Uno o más pasajeros no pertenecen a esta reservación.', 1;
        END

        UPDATE pas
        SET pas.CheckedLuggage = p.NewCheckedLuggage
        FROM dbo.Passenger pas
        JOIN @Passengers p ON p.Id = pas.Id

        UPDATE dbo.booking
        SET total_amount = total_amount + @TotalAmountPaid
        WHERE confirmation_code = UPPER(LTRIM(RTRIM(@ConfirmationCode)))

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO