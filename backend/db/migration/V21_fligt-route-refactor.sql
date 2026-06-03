EXEC sp_rename N'dbo.flight', N'route';
GO

EXEC sp_rename N'dbo.df_flight_frequency', N'df_route_frequency', N'OBJECT';
GO

EXEC sp_rename N'dbo.ck_flight_frequency_day_bits', N'ck_route_frequency_day_bits', N'OBJECT';
GO

EXEC sp_rename N'dbo.PurchaseOrder.FlightId', N'RouteId', N'COLUMN';
GO
