-- Indice para acelerar la busqueda por token de cancelacion en sp_CancelBooking

CREATE INDEX IX_booking_cancellation_token
ON dbo.booking (cancellation_token)
INCLUDE (cancellation_token_expires_at, status);