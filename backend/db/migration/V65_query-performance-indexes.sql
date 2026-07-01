CREATE INDEX IX_route_departure_lookup
ON dbo.[route] (departure_airport_id, is_deleted, departure_time);
GO

CREATE INDEX IX_route_arrival_lookup
ON dbo.[route] (arrival_airport_id, is_deleted, arrival_time, departure_time);
GO
